using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lottery
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public static string url = Environment.CurrentDirectory.ToString();//获取根目录的路径
        public static List<User> list_user = new List<User>();//用户列表
        public static List<string> list_jj = new List<string>();//获奖用户列表
        public static BindingList<Win> list_win = new BindingList<Win>();//获奖用户的展示列表
        public static DataTable dt = new DataTable();//存储数据集

        public static bool flag = true;//开关判断

        //开始抽奖
        private void Button3_Click(object sender, EventArgs e)
        {
            if (flag)
            {
                //开始抽奖
                flag = false;
                comboBox1.Enabled = false;

                timer1.Start();
                button3.Text = "暂停";
                lbl_message.Text = "抽奖中...";
            }
            else
            {
                //停止抽奖
                flag = true;
                timer1.Stop();
                comboBox1.Enabled = true;

                //判断抽到的结果
                IsFirstPrize();

                button3.Text = "开始抽奖";

            }
        }
        /// <summary>
        /// 获取当前奖项的列数
        /// </summary>
        /// <param name="name">奖项的名称</param>
        /// <returns>坐标</returns>
        private int GetIndex(string name)
        {
            int x = 0;
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                if (name == dt.Columns[i].ColumnName)
                {
                    x = i;
                    return i;
                }
            }
            return x;
        }

        private void IsFirstPrize()
        {
            try
            {
            string jj = comboBox1.SelectedItem.ToString();//获取当前奖项
         
            int x = GetIndex(jj);//获取当前奖项的坐标

           #region 展示中奖的图片
                string path = url + "//image//" + x + ".jpg";
                pictureBox1.Load(path);
                #endregion

            List<User> list_user2 = list_user.Where(o => (o.Star <= x && o.End >= x || o.Star == 0)).ToList();//获取参与抽奖的人数
            //判断用户人数是否已经抽完
            if (list_user2.Count > 0)
            {
            #region 暗箱操作(违规中)
            Random random = new Random();
            lbl_num1.Text = list_user2[random.Next(0, list_user2.Count)].Name;
                    #endregion

            #region 修改中奖的参数

                    lbl_message.Text = "恭喜" + lbl_num1.Text + "中奖了！";
                    int a_count = int.Parse(dt.Rows[1][x].ToString());//获取已经抽到的次数
                    int s_count = int.Parse(dt.Rows[2][x].ToString());//获取未抽到的次数

                    if (a_count == 0)
                    {
                        dt.Rows[1][x] = 1;
                        dt.Rows[2][x] = s_count - 1;
                    }
                    else
                    {
                        dt.Rows[1][x] = a_count + 1;
                        dt.Rows[2][x] = s_count - 1;
                    }

                    if (s_count - 1 == 0)
                    {
                        list_jj.Remove(jj);
                        InitComBox();
                    }
                    dataGridView2.DataSource = dt;
                    #endregion

            list_win.Add(new Win(jj, lbl_num1.Text)); //添加中奖的用户

            #region 从用户列表中删除中奖的用户


                    User user = list_user.Where(o => o.Name == lbl_num1.Text).FirstOrDefault();
            if (list_user.Count > 0)
            {
                list_user.Remove(user);
            }
            else
            {
                MessageBox.Show("名单已经抽完了，抽奖结束！");
            }
                    #endregion

            #region 将中奖的用户展示在控件中
                    list_win = new BindingList<Win>(list_win.OrderByDescending(o => o.Id).ToList());
                    dataGridView1.DataSource = list_win;
                    #endregion
                }
            else
                {
                    MessageBox.Show("名单已经抽完了！");
                    lbl_num1.Text = "";
                    lbl_message.Text = "抽奖结束！";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }


            if (list_user.Count == 0)
            {
                button3.Enabled = false;
                MessageBox.Show("名单已经抽完了！");
                lbl_num1.Text = "";
                lbl_message.Text = "抽奖结束！";
            }
            else
            {
                button3.Enabled = true;
            }
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            Random random = new Random();
            lbl_num1.Text = list_user[random.Next(0, list_user.Count)].Name;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitUserData();

            InitGv2();

            InitComBox();

            Init();
        }

        #region 默认样式设置
        private void Init()
        {
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView2.ClearSelection();
            dataGridView2.RowHeadersVisible = false;
            dataGridView2.AllowUserToAddRows = false;
            this.dataGridView2.ScrollBars = ScrollBars.None;//滚动条去除
            this.dataGridView2.Height = this.dataGridView2.Columns[0].HeaderCell.Size.Height * 4;
        }
        #endregion

        #region 初始化奖项列表
        private void InitComBox()
        {
            comboBox1.DataSource = null;
            comboBox1.DataSource = list_jj;
            comboBox1.SelectedIndex = list_jj.Count - 1;
        }
        #endregion

        #region 初始化奖项数量变化
        private void InitGv2()
        {
           
            DataRow dr = dt.NewRow();

            string path = url + "\\Config.xlsx";
            using (FileStream fs = File.OpenRead(path))   //打开myxls.xls文件
            {
                XSSFWorkbook wk = new XSSFWorkbook(fs);   //把xls文件中的数据写入wk中
                for (int i = 0; i < wk.NumberOfSheets; i++)  //NumberOfSheets是myxls.xls中总共的表数
                {
                    ISheet sheet = wk.GetSheetAt(i);   //读取当前表数据
                    for (int j = 0; j <= sheet.LastRowNum; j++)  //LastRowNum 是当前表的总行数
                    {
                        IRow row = sheet.GetRow(j);  //读取当前行数据
                        if (row != null)
                        {
                            if (j!=0)
                            {
                                dr = dt.NewRow();
                            }
                            for (int k = 0; k <= row.LastCellNum; k++)  //LastCellNum 是当前行的总列数
                            {
                                ICell cell = row.GetCell(k);  //当前表格
                                if (cell != null)
                                {
                                    string name = cell.ToString();
                                    if (j==0)
                                    {
                                        dt.Columns.Add(name);
                                        if (k!=0)
                                        {
                                            list_jj.Add(name);
                                        }
                                    }
                                    else
                                    {
                                        if (k < row.LastCellNum)
                                        {
                                          
                                            string cellname = dt.Columns[k].ColumnName.ToString();
                                            dr[cellname] = name;
                                        }
                                    }
                                }
                                else
                                {
                                 
                                }
                            }
                            if (j != 0)
                            {
                                dt.Rows.Add(dr);
                            }
                        }

                    }
                }
            }
            dataGridView2.DataSource = dt;
        }
        #endregion
      
        #region 初始化人员名单信息
        public void InitUserData()
        {
            string path = url + "\\User.xlsx";
            using (FileStream fs = File.OpenRead(path))   //打开myxls.xls文件
            {
                XSSFWorkbook wk = new XSSFWorkbook(fs);   //把xls文件中的数据写入wk中
                for (int i = 0; i < wk.NumberOfSheets; i++)  //NumberOfSheets是myxls.xls中总共的表数
                {
                    ISheet sheet = wk.GetSheetAt(i);   //读取当前表数据
                    for (int j = 1; j <= sheet.LastRowNum; j++)  //LastRowNum 是当前表的总行数
                    {
                        IRow row = sheet.GetRow(j);  //读取当前行数据
                        if (row != null)
                        {
                          
                                ICell cell = row.GetCell(0);  //当前表格
                                ICell cell2 = row.GetCell(1);  //当前表格
                               ICell cell3 = row.GetCell(2);  //当前表格
                            if (cell != null&& cell2!=null)
                                {
                                    string name = cell.ToString();
                                    list_user.Add(new User(name,int.Parse(cell2.ToString()), int.Parse(cell3.ToString())));
                            }

                        }
                    }
                }
            }
        }
        #endregion
      
        /// <summary>
        /// 导出中奖的名单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button1_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            //设置文件标题
            saveFileDialog.Title = "导出Excel文件";
            //设置文件类型
            saveFileDialog.Filter = "Excel 工作簿(*.xlsx)|*.xlsx|Excel 97-2003 工作簿(*.xls)|*.xls";
            //设置默认文件类型显示顺序  
            saveFileDialog.FilterIndex = 1;
            //是否自动在文件名中添加扩展名
            saveFileDialog.AddExtension = true;
            //是否记忆上次打开的目录
            saveFileDialog.RestoreDirectory = true;
            //设置默认文件名
            saveFileDialog.FileName = "中奖名单表.xlsx";
            //按下确定选择的按钮  
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string localFilePath = saveFileDialog.FileName.ToString();
                XSSFWorkbook excelBook = new XSSFWorkbook();
                ICellStyle style = excelBook.CreateCellStyle();
                //创建Excel工作表 Sheet=故障码信息
                ISheet sheet1 = excelBook.CreateSheet("某某");
                IRow row1 = sheet1.CreateRow(0);
                //给标题的每一个单元格赋值
                row1.CreateCell(0).SetCellValue("编号");//0
                row1.CreateCell(1).SetCellValue("奖项");//0
                row1.CreateCell(2).SetCellValue("中奖人名");//0
                for (int i = 0; i < list_win.Count; i++)
                {
                    //sheet1.CreateRow(i).
                    //创建行
                    IRow rowTemp = sheet1.CreateRow(i + 1);
                  
                    //故障码DTC
                    rowTemp.CreateCell(0).SetCellValue(list_win[i].Id);
                    rowTemp.CreateCell(1).SetCellValue(list_win[i].Awards);
                    rowTemp.CreateCell(2).SetCellValue(list_win[i].UserName);
                }
                using (FileStream fs = new FileStream(localFilePath, FileMode.Create, FileAccess.Write))
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        excelBook.Write(stream);
                        var buf = stream.ToArray();
                        fs.Write(buf, 0, buf.Length);
                    }
                }
            }
            
        }

    }
}
