namespace IceWallet.Client
{
    partial class MainForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.lbl_height = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel4 = new System.Windows.Forms.ToolStripStatusLabel();
            this.lbl_count_node = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.钱包WToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.新建钱包NToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.打开钱包OToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.退出XToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.创建新地址NToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.导入私钥IToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.查看私钥VToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.复制到剪贴板CToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.删除DToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label1 = new System.Windows.Forms.Label();
            this.lbl_balance = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.btn_debug = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.statusStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.lbl_height,
            this.toolStripStatusLabel4,
            this.lbl_count_node});
            this.statusStrip1.Location = new System.Drawing.Point(0, 397);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(619, 22);
            this.statusStrip1.SizingGrip = false;
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(35, 17);
            this.toolStripStatusLabel1.Text = "高度:";
            // 
            // lbl_height
            // 
            this.lbl_height.Name = "lbl_height";
            this.lbl_height.Size = new System.Drawing.Size(27, 17);
            this.lbl_height.Text = "0/0";
            // 
            // toolStripStatusLabel4
            // 
            this.toolStripStatusLabel4.Name = "toolStripStatusLabel4";
            this.toolStripStatusLabel4.Size = new System.Drawing.Size(47, 17);
            this.toolStripStatusLabel4.Text = "连接数:";
            // 
            // lbl_count_node
            // 
            this.lbl_count_node.Name = "lbl_count_node";
            this.lbl_count_node.Size = new System.Drawing.Size(15, 17);
            this.lbl_count_node.Text = "0";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.钱包WToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(619, 25);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // 钱包WToolStripMenuItem
            // 
            this.钱包WToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.新建钱包NToolStripMenuItem,
            this.打开钱包OToolStripMenuItem,
            this.toolStripSeparator1,
            this.退出XToolStripMenuItem});
            this.钱包WToolStripMenuItem.Name = "钱包WToolStripMenuItem";
            this.钱包WToolStripMenuItem.Size = new System.Drawing.Size(64, 21);
            this.钱包WToolStripMenuItem.Text = "钱包(&W)";
            // 
            // 新建钱包NToolStripMenuItem
            // 
            this.新建钱包NToolStripMenuItem.Name = "新建钱包NToolStripMenuItem";
            this.新建钱包NToolStripMenuItem.Size = new System.Drawing.Size(151, 22);
            this.新建钱包NToolStripMenuItem.Text = "新建钱包(&N)...";
            this.新建钱包NToolStripMenuItem.Click += new System.EventHandler(this.新建钱包NToolStripMenuItem_Click);
            // 
            // 打开钱包OToolStripMenuItem
            // 
            this.打开钱包OToolStripMenuItem.Name = "打开钱包OToolStripMenuItem";
            this.打开钱包OToolStripMenuItem.Size = new System.Drawing.Size(151, 22);
            this.打开钱包OToolStripMenuItem.Text = "打开钱包(&O)...";
            this.打开钱包OToolStripMenuItem.Click += new System.EventHandler(this.打开钱包OToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(148, 6);
            // 
            // 退出XToolStripMenuItem
            // 
            this.退出XToolStripMenuItem.Name = "退出XToolStripMenuItem";
            this.退出XToolStripMenuItem.Size = new System.Drawing.Size(151, 22);
            this.退出XToolStripMenuItem.Text = "退出(&X)";
            this.退出XToolStripMenuItem.Click += new System.EventHandler(this.退出XToolStripMenuItem_Click);
            // 
            // listView1
            // 
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.listView1.ContextMenuStrip = this.contextMenuStrip1;
            this.listView1.Font = new System.Drawing.Font("Lucida Console", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.listView1.FullRowSelect = true;
            this.listView1.GridLines = true;
            this.listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(12, 28);
            this.listView1.Name = "listView1";
            this.listView1.ShowGroups = false;
            this.listView1.Size = new System.Drawing.Size(595, 330);
            this.listView1.TabIndex = 2;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "地址";
            this.columnHeader1.Width = 280;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "余额";
            this.columnHeader2.Width = 120;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.创建新地址NToolStripMenuItem,
            this.导入私钥IToolStripMenuItem,
            this.toolStripSeparator2,
            this.查看私钥VToolStripMenuItem,
            this.复制到剪贴板CToolStripMenuItem,
            this.删除DToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(157, 120);
            // 
            // 创建新地址NToolStripMenuItem
            // 
            this.创建新地址NToolStripMenuItem.Enabled = false;
            this.创建新地址NToolStripMenuItem.Name = "创建新地址NToolStripMenuItem";
            this.创建新地址NToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.创建新地址NToolStripMenuItem.Text = "创建新地址(&N)";
            this.创建新地址NToolStripMenuItem.Click += new System.EventHandler(this.创建新地址NToolStripMenuItem_Click);
            // 
            // 导入私钥IToolStripMenuItem
            // 
            this.导入私钥IToolStripMenuItem.Enabled = false;
            this.导入私钥IToolStripMenuItem.Name = "导入私钥IToolStripMenuItem";
            this.导入私钥IToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.导入私钥IToolStripMenuItem.Text = "导入私钥(&I)...";
            this.导入私钥IToolStripMenuItem.Click += new System.EventHandler(this.导入私钥IToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(153, 6);
            // 
            // 查看私钥VToolStripMenuItem
            // 
            this.查看私钥VToolStripMenuItem.Enabled = false;
            this.查看私钥VToolStripMenuItem.Name = "查看私钥VToolStripMenuItem";
            this.查看私钥VToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.查看私钥VToolStripMenuItem.Text = "查看私钥(&V)";
            this.查看私钥VToolStripMenuItem.Click += new System.EventHandler(this.查看私钥VToolStripMenuItem_Click);
            // 
            // 复制到剪贴板CToolStripMenuItem
            // 
            this.复制到剪贴板CToolStripMenuItem.Enabled = false;
            this.复制到剪贴板CToolStripMenuItem.Name = "复制到剪贴板CToolStripMenuItem";
            this.复制到剪贴板CToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.复制到剪贴板CToolStripMenuItem.ShowShortcutKeys = false;
            this.复制到剪贴板CToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.复制到剪贴板CToolStripMenuItem.Text = "复制到剪贴板(&C)";
            this.复制到剪贴板CToolStripMenuItem.Click += new System.EventHandler(this.复制到剪贴板CToolStripMenuItem_Click);
            // 
            // 删除DToolStripMenuItem
            // 
            this.删除DToolStripMenuItem.Enabled = false;
            this.删除DToolStripMenuItem.Name = "删除DToolStripMenuItem";
            this.删除DToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.删除DToolStripMenuItem.Text = "删除(&D)...";
            this.删除DToolStripMenuItem.Click += new System.EventHandler(this.删除DToolStripMenuItem_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(12, 367);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 21);
            this.label1.TabIndex = 3;
            this.label1.Text = "总余额:";
            // 
            // lbl_balance
            // 
            this.lbl_balance.AutoSize = true;
            this.lbl_balance.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbl_balance.Location = new System.Drawing.Point(80, 367);
            this.lbl_balance.Name = "lbl_balance";
            this.lbl_balance.Size = new System.Drawing.Size(19, 21);
            this.lbl_balance.TabIndex = 4;
            this.lbl_balance.Text = "0";
            // 
            // button1
            // 
            this.button1.Enabled = false;
            this.button1.Location = new System.Drawing.Point(532, 368);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 5;
            this.button1.Text = "刷新";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btn_debug
            // 
            this.btn_debug.Location = new System.Drawing.Point(451, 368);
            this.btn_debug.Name = "btn_debug";
            this.btn_debug.Size = new System.Drawing.Size(75, 23);
            this.btn_debug.TabIndex = 6;
            this.btn_debug.Text = "调试";
            this.btn_debug.UseVisualStyleBackColor = true;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(619, 419);
            this.Controls.Add(this.btn_debug);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.lbl_balance);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "IceWallet";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel lbl_height;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel4;
        private System.Windows.Forms.ToolStripStatusLabel lbl_count_node;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 钱包WToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 新建钱包NToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 打开钱包OToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem 退出XToolStripMenuItem;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lbl_balance;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 创建新地址NToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 删除DToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem 查看私钥VToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 导入私钥IToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 复制到剪贴板CToolStripMenuItem;
        private System.Windows.Forms.Button btn_debug;
        private System.Windows.Forms.Timer timer1;
    }
}

