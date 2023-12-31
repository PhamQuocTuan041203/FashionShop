﻿using BUS;
using DTO;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace GUI
{
    public partial class frmProduct : Form
    {
        private string fileAddress;
        private byte[] img;
        BUS_Product busProduct = new BUS_Product();
        DTO_Product dtoProduct;

        public frmProduct()
        {
            InitializeComponent();
        }

        private void SetValue(bool param, bool isLoad)
        {
            txtId.Text = null;
            txtName.Focus();
            txtName.Text = null;
            txtQuantity.Text = null;
            txtUnitPrice.Text = null;
            txtImportUnitPrice.Text = null;
            txtNote.Text = null;

            btnInsert.Enabled = param;
            btnInsert.Enabled = true;
            pcbProduct.Image = null;

            if (isLoad)
            {
                btnUpdate.Enabled = false;
                btnDelete.Enabled = false;
            }
            else
            {
                btnUpdate.Enabled = !param;
                btnDelete.Enabled = !param;
            }
        }

        private void MsgBox(string message, bool isError = false)
        {
            if (isError)
                MessageBox.Show(message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
                MessageBox.Show(message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private Image CloneImage(string path)
        {
            Image result;
            using (Bitmap original = new Bitmap(path))
            {
                result = (Bitmap)original.Clone();

            };
            return result;
        }

        private byte[] ImageToByteArray(PictureBox pictureBox)
        {
            MemoryStream memoryStream = new MemoryStream();
            pictureBox.Image.Save(memoryStream, pictureBox.Image.RawFormat);
            return memoryStream.ToArray();
        }

        private void LoadGridView()
        {
            gvProduct.Columns[0].HeaderText = "Mã";
            gvProduct.Columns[1].HeaderText = "Tên";
            gvProduct.Columns[2].HeaderText = "SL";
            gvProduct.Columns[3].HeaderText = "Giá nhập";
            gvProduct.Columns[4].HeaderText = "Giá bán";
            gvProduct.Columns[5].HeaderText = "Hình ảnh";
            gvProduct.Columns[6].HeaderText = "Ghi chú";

            gvProduct.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            gvProduct.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            gvProduct.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            gvProduct.Columns[4].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            
            foreach (DataGridViewColumn item in gvProduct.Columns)
                item.DividerWidth = 1;

            DataGridViewImageColumn imgCol = new DataGridViewImageColumn();
            imgCol = (DataGridViewImageColumn)gvProduct.Columns[5];
            imgCol.ImageLayout = DataGridViewImageCellLayout.Zoom;

            gvProduct.RowTemplate.Height = 90;
            gvProduct.Columns[0].Width = 80;
            gvProduct.Columns[1].Width = 300;
            gvProduct.Columns[2].Width = 60;
            gvProduct.Columns[3].Width = 100;
            gvProduct.Columns[4].Width = 100;
            gvProduct.Columns[5].Width = 90;
        }

        private bool CheckIsNummber(string text)
        {
            if (int.TryParse(text, out int number))
            {
                return number >= 0;
            }
            return false;
        }

        private void OpenImage()
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Pictures files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png)|*.jpg; *.jpeg; *.jpe; *.jfif; *.png|All files (*.*)|*.*";
            open.Title = "Chọn ảnh";

            if (open.ShowDialog() == DialogResult.OK)
            {
                fileAddress = open.FileName;
                pcbProduct.Image = CloneImage(fileAddress);
                pcbProduct.ImageLocation = fileAddress;
                img = ImageToByteArray(pcbProduct);
            }
        }

        private void frmProduct_Load(object sender, EventArgs e)
        {
            gvProduct.DataSource = busProduct.ListOfProducts();
            LoadGridView();
            SetValue(true, false);
            txtName.Focus();
        }

        private void btnInsertPicture_Click(object sender, EventArgs e)
        {
            OpenImage();
        }

        private void gvProduct_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (gvProduct.Rows.Count > 0)
            {
                btnInsert.Enabled = false;
                btnUpdate.Enabled = true;
                btnDelete.Enabled = true;

                txtId.ReadOnly = true;
                txtId.Text = gvProduct.CurrentRow.Cells[0].Value.ToString();
                txtName.Text = gvProduct.CurrentRow.Cells[1].Value.ToString();
                txtQuantity.Text = gvProduct.CurrentRow.Cells[2].Value.ToString();
                txtImportUnitPrice.Text = gvProduct.CurrentRow.Cells[3].Value.ToString();
                txtUnitPrice.Text = gvProduct.CurrentRow.Cells[4].Value.ToString();

                MemoryStream memoryStream = new MemoryStream((byte[])gvProduct.CurrentRow.Cells[5].Value);
                pcbProduct.Image = Image.FromStream(memoryStream);
                txtNote.Text = gvProduct.CurrentRow.Cells[6].Value.ToString();
            }
        }

        private void btnInsert_Click(object sender, EventArgs e)
        {
            if (!CheckIsNummber(txtQuantity.Text) || !CheckIsNummber(txtUnitPrice.Text) || !CheckIsNummber(txtImportUnitPrice.Text))
                MsgBox("Vui lòng nhập số đúng định dạng!", true);
            else if (txtName.Text == "")
                MsgBox("Vui lòng nhập tên sản phẩm!", true);
            else if (pcbProduct.Image == null)
                MsgBox("Vui lòng chọn hình!", true);
            else
            {
                dtoProduct = new DTO_Product
                (
                    txtName.Text,
                    int.Parse(txtQuantity.Text),
                    int.Parse(txtImportUnitPrice.Text),
                    int.Parse(txtUnitPrice.Text),
                    ImageToByteArray(pcbProduct),
                    txtNote.Text
                );
                if (busProduct.InsertProduct(dtoProduct))
                {
                    gvProduct.DataSource = busProduct.ListOfProducts();
                    LoadGridView();
                    SetValue(true, false);
                    MsgBox("Thêm sản phẩm thành công!");
                }
                else
                    MsgBox("Không thể thêm sản phẩm!", true);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Bạn có chắc muốn sửa?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (!CheckIsNummber(txtQuantity.Text) || !CheckIsNummber(txtUnitPrice.Text) || !CheckIsNummber(txtImportUnitPrice.Text))
                    MsgBox("Vui lòng nhập số đúng định dạng!", true);
                else if (txtName.Text == "")
                    MsgBox("Vui lòng nhập tên sản phẩm!", true);
                else if (pcbProduct.Image == null)
                    MsgBox("Vui lòng chọn hình ảnh!", true);
                else
                {
                    dtoProduct = new DTO_Product
                    (
                        int.Parse(txtId.Text),
                        txtName.Text,
                        int.Parse(txtQuantity.Text),
                        int.Parse(txtImportUnitPrice.Text),
                        int.Parse(txtUnitPrice.Text),
                        ImageToByteArray(pcbProduct),
                        txtNote.Text
                    );
                    if (busProduct.UpdateProduct(dtoProduct))
                    {
                        gvProduct.DataSource = busProduct.ListOfProducts();
                        LoadGridView();
                        SetValue(true, false);
                        MsgBox("Sửa sản phẩm thành công!");
                    }
                    else
                    {
                        MsgBox("Không thể sửa sản phẩm!", true);
                    }
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Bạn có chắc muốn xóa không ?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (busProduct.DeleteProduct(int.Parse(txtId.Text)))
                {
                    gvProduct.DataSource = busProduct.ListOfProducts();
                    LoadGridView();
                    MsgBox("Xóa thành công");
                }
                else
                    MsgBox("Không thể xóa sản phẩm!");
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            gvProduct.DataSource = busProduct.ListOfProducts();
            LoadGridView();
            SetValue(true, false);
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            string name = txtSearch.Text.Trim();
            if (name == "")
            {
                frmProduct_Load(sender, e);
                txtSearch.Focus();
            }
            else
            {
                DataTable data = busProduct.SearchProduct(txtSearch.Text);
                gvProduct.DataSource = data;
            }
        }
    }
}
