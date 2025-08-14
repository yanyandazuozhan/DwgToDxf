using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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

        }
    }
}
