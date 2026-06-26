namespace CSiNET8PluginExample1
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.btnExtract = new System.Windows.Forms.Button();
            this.panelProperties = new System.Windows.Forms.Panel();
            this.btnPushToEtabs = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblDeflection = new System.Windows.Forms.Label();
            this.lblThickness = new System.Windows.Forms.Label();
            this.lblSlabName = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.panelProperties.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(12, 12);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowTemplate.Height = 25;
            this.dataGridView1.Size = new System.Drawing.Size(650, 450);
            this.dataGridView1.TabIndex = 0;
            this.dataGridView1.SelectionChanged += new System.EventHandler(this.dataGridView1_SelectionChanged);
            // 
            // btnExtract
            // 
            this.btnExtract.Location = new System.Drawing.Point(12, 475);
            this.btnExtract.Name = "btnExtract";
            this.btnExtract.Size = new System.Drawing.Size(650, 40);
            this.btnExtract.TabIndex = 1;
            this.btnExtract.Text = "Extract and Design Slabs";
            this.btnExtract.UseVisualStyleBackColor = true;
            this.btnExtract.Click += new System.EventHandler(this.button1_Click);
            // 
            // panelProperties
            // 
            this.panelProperties.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelProperties.Controls.Add(this.btnPushToEtabs);
            this.panelProperties.Controls.Add(this.lblStatus);
            this.panelProperties.Controls.Add(this.lblDeflection);
            this.panelProperties.Controls.Add(this.lblThickness);
            this.panelProperties.Controls.Add(this.lblSlabName);
            this.panelProperties.Location = new System.Drawing.Point(670, 12);
            this.panelProperties.Name = "panelProperties";
            this.panelProperties.Size = new System.Drawing.Size(300, 450);
            this.panelProperties.TabIndex = 2;
            // 
            // btnPushToEtabs
            // 
            this.btnPushToEtabs.Location = new System.Drawing.Point(15, 200);
            this.btnPushToEtabs.Name = "btnPushToEtabs";
            this.btnPushToEtabs.Size = new System.Drawing.Size(265, 40);
            this.btnPushToEtabs.TabIndex = 4;
            this.btnPushToEtabs.Text = "Push New Thickness to ETABS";
            this.btnPushToEtabs.UseVisualStyleBackColor = true;
            this.btnPushToEtabs.Click += new System.EventHandler(this.btnPushToEtabs_Click);
            this.btnPushToEtabs.Enabled = false;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(15, 120);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(42, 15);
            this.lblStatus.TabIndex = 3;
            this.lblStatus.Text = "Status: -";
            // 
            // lblDeflection
            // 
            this.lblDeflection.AutoSize = true;
            this.lblDeflection.Location = new System.Drawing.Point(15, 90);
            this.lblDeflection.Name = "lblDeflection";
            this.lblDeflection.Size = new System.Drawing.Size(65, 15);
            this.lblDeflection.TabIndex = 2;
            this.lblDeflection.Text = "Deflection: -";
            // 
            // lblThickness
            // 
            this.lblThickness.AutoSize = true;
            this.lblThickness.Location = new System.Drawing.Point(15, 60);
            this.lblThickness.Name = "lblThickness";
            this.lblThickness.Size = new System.Drawing.Size(61, 15);
            this.lblThickness.TabIndex = 1;
            this.lblThickness.Text = "Thickness: -";
            // 
            // lblSlabName
            // 
            this.lblSlabName.AutoSize = true;
            this.lblSlabName.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblSlabName.Location = new System.Drawing.Point(15, 15);
            this.lblSlabName.Name = "lblSlabName";
            this.lblSlabName.Size = new System.Drawing.Size(126, 21);
            this.lblSlabName.TabIndex = 0;
            this.lblSlabName.Text = "Select a Slab";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 526);
            this.Controls.Add(this.panelProperties);
            this.Controls.Add(this.btnExtract);
            this.Controls.Add(this.dataGridView1);
            this.Name = "Form1";
            this.Text = "ETABS Slab Designer";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.panelProperties.ResumeLayout(false);
            this.panelProperties.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button btnExtract;
        private System.Windows.Forms.Panel panelProperties;
        private System.Windows.Forms.Button btnPushToEtabs;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblDeflection;
        private System.Windows.Forms.Label lblThickness;
        private System.Windows.Forms.Label lblSlabName;
    }
}
