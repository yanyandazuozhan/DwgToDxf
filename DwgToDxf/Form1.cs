using netDxf;
using netDxf.Entities;
using netDxf.Tables;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static netDxf.Entities.HatchBoundaryPath;


namespace DwgToDxf
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        string filePath = "";
        private void BtnChooseFile_Click(object sender, EventArgs e)
        {
            // 创建 OpenFileDialog 实例
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // 设置对话框属性
            openFileDialog.Title = "选择文件"; // 对话框标题
            openFileDialog.InitialDirectory = @"C:\Users\jack1\Downloads"; // 初始目录
            openFileDialog.Filter = "dwg文件 (*.dwg)|*.dwg|所有文件 (*.*)|*.*"; // 文件过滤器
            openFileDialog.FilterIndex = 1; // 默认过滤器索引
            openFileDialog.RestoreDirectory = true; // 关闭对话框后恢复当前目录
            openFileDialog.Multiselect = false; // 是否允许多选

            // 显示对话框并检查用户是否点击了"打开"
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // 获取选择的文件路径
                     filePath = openFileDialog.FileName;

                    // 在这里处理文件，例如读取内容
                    // string fileContent = File.ReadAllText(filePath);

                    // 显示文件路径（示例）
                    textBox1.Text= filePath;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"打开文件时出错: {ex.Message}", "错误",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        
        }

        private void btnConvert_Click(object sender, EventArgs e)
        {
            string path = "D:\\liuyan\\project\\cad\\input\\404PS套料图-君正25900-7.dxf";
            DxfDocument doc = DxfDocument.Load(path, new List<string> { @".\Support" });

            var result = doc.Entities.Polylines2D.Where(m => m.Vertexes.Count == 4 &&m.Linetype.Name== "CONTINUOUS").FirstOrDefault();
            double area = 0;
            if (result != null)
            {
                area=GetResult(result);

            }
            var firstquery = from n in doc.Entities.Polylines2D
                             where n.Vertexes.Count == 4 && n.Linetype.Name == "CONTINUOUS"
                             select n;

            var finalresult = firstquery.Where(t => GetResult(t) == area);

         //   Polyline2D firstpolyline = finalresult.FirstOrDefault();//符合周长=1344的某个多段线=>即某个单独图表
            List<string> names=new List<string>();
            foreach (var firstpolyline in finalresult)
            {
                DxfDocument demoDoc = new DxfDocument();
                //获取该图像的范围
                var gdbPlateMinX = firstpolyline.Vertexes.OrderBy(m => m.Position.X).FirstOrDefault().Position.X;
                var gdbPlateMaxX = firstpolyline.Vertexes.OrderByDescending(m => m.Position.X).FirstOrDefault().Position.X;
                var gdbPlateMinY = firstpolyline.Vertexes.OrderBy(m => m.Position.Y).FirstOrDefault().Position.Y;
                var gdbPlateMaxY = firstpolyline.Vertexes.OrderByDescending(m => m.Position.Y).FirstOrDefault().Position.Y;
                List<Polyline2D> all_polys = doc.Entities.Polylines2D.ToList();//poly2d

                List<Text> txts = doc.Entities.Texts.ToList();
                List<MText> mtxts = doc.Entities.MTexts.ToList();
                

                //多段线
                foreach (var polyline2D in all_polys)
                {
                    for (int j = 0; j < polyline2D.Vertexes.Count; j++)
                    {
                        if (polyline2D.Vertexes[j].Position.X > gdbPlateMinX && polyline2D.Vertexes[j].Position.X < gdbPlateMaxX &&
                            polyline2D.Vertexes[j].Position.Y > gdbPlateMinY && polyline2D.Vertexes[j].Position.Y < gdbPlateMaxY)
                        {
                            //bIsIn = true;
                            Polyline2D lineclone = (Polyline2D)polyline2D.Clone();
                            demoDoc.Entities.Add(lineclone);
                        }
                        else
                        {
                            //bIsIn = false;
                            //break;
                        }
                    }
                }
                List<Text> targetTXTContents = new List<Text>();//origin
                List<string> targetTXTContents_clone = new List<string>();//clone
                                                                          //文字


            // 在循环外部提前创建支持中文的文本样式
            // 在循环外部创建中文字体样式
            // 确保使用系统中已安装的TrueType中文字体


                 foreach (var text in txts)
                 {
               
              
                     if (text.Position.X > gdbPlateMinX && text.Position.X < gdbPlateMaxX && text.Position.Y > gdbPlateMinY && text.Position.Y < gdbPlateMaxY)
                     {
                             string con = text.Value;
                             targetTXTContents.Add(text);//=>提取出这个图表的所有文字集合
                             Text te = (Text)text.Clone();
                         // 设置支持中文的字体
                      
                         demoDoc.Entities.Add(te);
                     }
                 }
                 //求取目标方框的坐标范围=>提取出文字后用正则表达式匹配或者是上一个+1这种
                 var tagetminX = gdbPlateMinX+354;
                 var tagetminY = gdbPlateMinY+254;//计算图表
              
                 var tagetmaxX = gdbPlateMaxX+383;
                 var tagetmaxY = gdbPlateMaxY+262;//计算图标
              
                 foreach (var text in targetTXTContents)
                 {
                     //if判断坐标区间
                     if (text.Position.X > tagetminX && text.Position.X < tagetmaxX && text.Position.Y > tagetminY && text.Position.Y < tagetmaxY&& text.Value.Contains("404PS"))
                     { 
                             string m= text.Value;
                             names.Add(m);
                     }
                 }
              
              
                 string b = "";
           
      
         
            demoDoc.Save("text.dxf");
                //MessageBox.Show("结束");
            }

            string a2 = "";
        }

        public double GetResult(Polyline2D result)
        {
            double area = 0;
            var minX = result.Vertexes.OrderBy(m => m.Position.X).FirstOrDefault().Position.X;
            var maxX = result.Vertexes.OrderByDescending(m => m.Position.X).FirstOrDefault().Position.X;
            var minY = result.Vertexes.OrderBy(m => m.Position.Y).FirstOrDefault().Position.Y;
            var maxY = result.Vertexes.OrderByDescending(m => m.Position.Y).FirstOrDefault().Position.Y;

            double dx = maxX - minX;
            double dy = maxY - minY;
            area = Math.Round((dx + dy) * 2);
            return area;    
        }

        
    }
}
