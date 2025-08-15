using netDxf;
using netDxf.Entities;
using netDxf.Header;
using netDxf.Tables;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static netDxf.Entities.HatchBoundaryPath;
using static System.Net.Mime.MediaTypeNames;
using Line = netDxf.Entities.Line;
using Text = netDxf.Entities.Text;

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
        public TextStyle textStyle { get; set; }
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
            var entityStats = doc.Entities.All
          .GroupBy(e1 => e1.Type)  // 按 EntityType 枚举分组
          .Select(g => new { Type = g.Key, Count = g.Count() });

            foreach (var stat in entityStats)
            {
                Console.WriteLine($"{stat.Type} : {stat.Count}");
            }
            var outputdir = "D:\\liuyan\\project\\cad\\output\\dxfs";

            DxfDocument newDxfFile1 = new DxfDocument();

            textStyle = doc.TextStyles.Where(m => m.Name.Contains("宋")).FirstOrDefault();
            foreach (TextStyle style in doc.TextStyles)
            {
                if (!newDxfFile1.TextStyles.Contains(style.Name))
                {
                    newDxfFile1.TextStyles.Add((TextStyle)style.Clone());
                }
            }

            //foreach (var item in finalresult)
            //{
            //    var text = FindText(doc, item);
            //    Console.WriteLine(  text);
            //}
           
            int x = 0;
            foreach (var item in finalresult)
            {
                x++;
                DxfDocument newDxfFile = new DxfDocument();

                var newdxf = AddElements(doc, item, newDxfFile);
                string file = FindText(doc, item);

                // 基础文件名（不含扩展名）
                string baseFileName = file;
                // 完整路径（初始值）
                string fileName = Path.Combine(outputdir, $"{baseFileName}.dxf");

                // 计数器，用于生成唯一文件名
                int counter = 1;

                // 检查文件是否存在，如果存在则添加"_1"后缀并重试
                while (File.Exists(fileName))
                {
                    // 生成新的文件名，如"file_1.dxf"、"file_1_1.dxf"等
                    fileName = Path.Combine(outputdir, $"{baseFileName}_1.dxf");
                    baseFileName = $"{baseFileName}_1"; // 更新基础文件名，以便下一次循环继续添加后缀
                    counter++;
                }

                // 保存文件
                newdxf.Save(fileName);
            }


            //newDxfFile.Save("test.dxf");
            MessageBox.Show("success");

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

        public string FindText(DxfDocument doc, Polyline2D polyline2D)
        {
            string retext = "";
            List<Text>list = new List<Text>();
            var txts = doc.Entities.Texts;


            var gdbPlateMinX = polyline2D.Vertexes.OrderBy(m => m.Position.X).FirstOrDefault().Position.X;
            var gdbPlateMaxX = polyline2D.Vertexes.OrderByDescending(m => m.Position.X).FirstOrDefault().Position.X;
            var gdbPlateMinY = polyline2D.Vertexes.OrderBy(m => m.Position.Y).FirstOrDefault().Position.Y;
            var gdbPlateMaxY = polyline2D.Vertexes.OrderByDescending(m => m.Position.Y).FirstOrDefault().Position.Y;
            foreach (var text in txts)
            {


                if (text.Position.X > gdbPlateMinX && text.Position.X < gdbPlateMaxX && text.Position.Y > gdbPlateMinY && text.Position.Y < gdbPlateMaxY)
                {
                    string con = text.Value;
                    list.Add(text);//=>提取出这个图表的所有文字集合

                }
            }
            //求取目标方框的坐标范围=>提取出文字后用正则表达式匹配或者是上一个+1这种
            var tagetminX = gdbPlateMinX + 354;
            var tagetminY = gdbPlateMinY + 254;//计算图表

            var tagetmaxX = gdbPlateMaxX + 383;
            var tagetmaxY = gdbPlateMaxY + 262;//计算图标

            foreach (var text in list)
            {
                //if判断坐标区间
                if (text.Position.X > tagetminX && text.Position.X < tagetmaxX && text.Position.Y > tagetminY && text.Position.Y < tagetmaxY && text.Value.Contains("404PS"))
                {
                    retext = text.Value;
                  
                }
            }
         
            //      var sortedTexts = list
            //.OrderByDescending(t => t.Position.Y)
            //.ThenBy(t => t.Position.X)
            //.ToList();
            //      var table = new List<List<string>>();


            //      // 初始化第一行
            //      var currentRow = new List<string> { sortedTexts[0].Value };
            //      double currentY = sortedTexts[0].Position.Y;

            //      // 遍历剩余文本进行分组
            //      for (int i = 1; i < sortedTexts.Count; i++)
            //      {
            //          var text = sortedTexts[i];
            //          // 判断是否和当前行在同一行（Y坐标差异在容差范围内）
            //          if (Math.Abs(text.Position.Y - currentY) <= 0)
            //          {
            //              currentRow.Add(text.Value);
            //          }
            //          else
            //          {
            //              // 新行
            //              table.Add(currentRow);
            //              currentRow = new List<string> { text.Value };
            //              currentY = text.Position.Y;
            //          }
            //      }

            //      // 添加最后一行
            //      table.Add(currentRow);
            //      //Console.WriteLine(table[1].Last());
            //      retext = table[1].Last();
            return retext;
        }
     
        public DxfDocument AddElements(DxfDocument doc, Polyline2D polyline2D,DxfDocument newDxfFile)
        {


            var gdbPlateMinX = polyline2D.Vertexes.OrderBy(m => m.Position.X).FirstOrDefault().Position.X;
            var gdbPlateMaxX = polyline2D.Vertexes.OrderByDescending(m => m.Position.X).FirstOrDefault().Position.X;
            var gdbPlateMinY = polyline2D.Vertexes.OrderBy(m => m.Position.Y).FirstOrDefault().Position.Y;
            var gdbPlateMaxY = polyline2D.Vertexes.OrderByDescending(m => m.Position.Y).FirstOrDefault().Position.Y;
            foreach (TextStyle style in doc.TextStyles)
            {
                if (!newDxfFile.TextStyles.Contains(style.Name))
                {
                    newDxfFile.TextStyles.Add((TextStyle)style.Clone());
                }
            }
            foreach (var item in doc.Entities.All)
            {


                if (item is Polyline2D)
                {
                    Polyline2D line = (Polyline2D)item;

                    for (int i = 0; i < line.Vertexes.Count; i++)
                    {
                        if (line.Vertexes[i].Position.X >= gdbPlateMinX && line.Vertexes[i].Position.X <= gdbPlateMaxX &&
                  line.Vertexes[i].Position.Y >= gdbPlateMinY && line.Vertexes[i].Position.Y <= gdbPlateMaxY)
                        {
                            Polyline2D cloned = (Polyline2D)line.Clone();
                            newDxfFile.Entities.Add(cloned);

                        }


                    }



                }

                if (item is netDxf.Entities.Text)
                {
                    netDxf.Entities.Text text = (netDxf.Entities.Text)item;
                    if (text.Position.X >= gdbPlateMinX && text.Position.X < gdbPlateMaxX && text.Position.Y >= gdbPlateMinY && text.Position.Y <= gdbPlateMaxY)
                    {
                        netDxf.Entities.Text cloned = (netDxf.Entities.Text)text.Clone();
                        cloned.Style = textStyle;

                        newDxfFile.Entities.Add(cloned);

                    }
                }

                //if (item is Dimension)
                //{

                //    Dimension dimension = (Dimension)item;
                   
                //    if (dimension is AlignedDimension)
                //    {
                //        AlignedDimension alignedDimension = (AlignedDimension)dimension;
                //        var f1 = alignedDimension.FirstReferencePoint;
                //        var f2 = alignedDimension.SecondReferencePoint;
                  
                //        if (f1.X >= gdbPlateMinX && f1.X < gdbPlateMaxX && f1.Y >= gdbPlateMinY && f1.Y <= gdbPlateMaxY &&
                //            f2.X >= gdbPlateMinX && f2.X < gdbPlateMaxX && f2.Y >= gdbPlateMinY && f2.Y <= gdbPlateMaxY)
                //        {
                //            AlignedDimension cloned = (AlignedDimension)alignedDimension.Clone();
                //            cloned.TextPositionManuallySet = true;
                //            cloned.FirstReferencePoint = f1;
                //            cloned.SecondReferencePoint = f2;
                //            Console.WriteLine(cloned.UserText);
                //            newDxfFile.Entities.Add(cloned);

                //        }
                //    }

                //}
                if (item is MText)
                {
                    netDxf.Entities.MText text = (netDxf.Entities.MText)item;
                    if (text.Position.X >= gdbPlateMinX && text.Position.X < gdbPlateMaxX && text.Position.Y >= gdbPlateMinY && text.Position.Y <= gdbPlateMaxY)
                    {
                        netDxf.Entities.MText cloned = (netDxf.Entities.MText)text.Clone();
                        newDxfFile.Entities.Add(cloned);

                    }
                }
                if (item is Line)
                {
                    Line line = (Line)item;
                    if (line.StartPoint.X >= gdbPlateMinX && line.StartPoint.X <= gdbPlateMaxX &&
                   line.StartPoint.Y >= gdbPlateMinY && line.StartPoint.Y <= gdbPlateMaxY &&
                   line.EndPoint.X >= gdbPlateMinX && line.EndPoint.X <= gdbPlateMaxX &&
                   line.EndPoint.Y >= gdbPlateMinY && line.EndPoint.Y <= gdbPlateMaxY)
                    {
                        Line cloned = (Line)line.Clone();
                        newDxfFile.Entities.Add(cloned);
                    }

                }
                if (item is Solid)
                {
                    Solid solid = (Solid)item;
                    Vector2 v1 = solid.FirstVertex;
                    Vector2 v2 = solid.SecondVertex;
                    Vector2 v3 = solid.ThirdVertex;
                    Vector2 v4 = solid.FourthVertex;
                    if (v1.X >= gdbPlateMinX && v1.X <= gdbPlateMaxX && v1.Y <= gdbPlateMaxY && v1.Y >= gdbPlateMinY &&
                        v2.X >= gdbPlateMinX && v2.X <= gdbPlateMaxX && v2.Y <= gdbPlateMaxY && v2.Y >= gdbPlateMinY &&
                        v3.X >= gdbPlateMinX && v3.X <= gdbPlateMaxX && v3.Y <= gdbPlateMaxY && v3.Y >= gdbPlateMinY &&
                        v4.X >= gdbPlateMinX && v4.X <= gdbPlateMaxX && v4.Y <= gdbPlateMaxY && v4.Y >= gdbPlateMinY)
                    {
                        Solid s = (Solid)solid.Clone();
                        newDxfFile.Entities.Add(s);
                    }
                }
                if (item is Circle)
                {
                    Circle circle = (Circle)item;
                    if (circle.Center.X >= gdbPlateMinX && circle.Center.X <= gdbPlateMaxX &&
                    circle.Center.Y >= gdbPlateMinY && circle.Center.Y <= gdbPlateMaxY)
                    {
                        Circle ci = (Circle)circle.Clone();
                        newDxfFile.Entities.Add(ci);
                    }



                }
            }

            return newDxfFile;
        }
            

            

    }
}
