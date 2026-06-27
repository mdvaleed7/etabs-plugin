// PATCH NOTES (v2):
//   • New input group on the right-hand panel: Fy, Cover, Bar Ø main / dist,
//     and an Fck override (used only when ETABS material lookup fails).
//   • Layout adjusted: data grid shrunk slightly to fit a wider input panel.
#nullable enable
namespace CSiNET8PluginExample1
{
    partial class Form1
    {
        private System.ComponentModel.IContainer? components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            this.dataGridView1   = new System.Windows.Forms.DataGridView();
            this.btnExtract      = new System.Windows.Forms.Button();
            this.panelProperties = new System.Windows.Forms.Panel();
            this.panelInputs     = new System.Windows.Forms.Panel();
            this.btnPushToEtabs  = new System.Windows.Forms.Button();
            this.lblStatus       = new System.Windows.Forms.Label();
            this.lblDeflection   = new System.Windows.Forms.Label();
            this.lblThickness    = new System.Windows.Forms.Label();
            this.lblSlabName     = new System.Windows.Forms.Label();

            // PATCH: user-input controls
            this.lblInputsHdr    = new System.Windows.Forms.Label();
            this.lblFy           = new System.Windows.Forms.Label();
            this.cmbFy           = new System.Windows.Forms.ComboBox();
            this.lblCover        = new System.Windows.Forms.Label();
            this.numCover        = new System.Windows.Forms.NumericUpDown();
            this.lblBarMain      = new System.Windows.Forms.Label();
            this.numBarMain      = new System.Windows.Forms.NumericUpDown();
            this.lblBarDist      = new System.Windows.Forms.Label();
            this.numBarDist      = new System.Windows.Forms.NumericUpDown();
            this.lblFckOverride  = new System.Windows.Forms.Label();
            this.numFckOverride  = new System.Windows.Forms.NumericUpDown();

            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numCover     )).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBarMain   )).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBarDist   )).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numFckOverride)).BeginInit();
            this.panelProperties.SuspendLayout();
            this.panelInputs    .SuspendLayout();
            this.SuspendLayout();

            // dataGridView1
            this.dataGridView1.Location = new System.Drawing.Point(12, 12);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(650, 450);
            this.dataGridView1.SelectionChanged += new System.EventHandler(this.dataGridView1_SelectionChanged);

            // btnExtract
            this.btnExtract.Location = new System.Drawing.Point(12, 475);
            this.btnExtract.Size = new System.Drawing.Size(650, 40);
            this.btnExtract.Text = "Extract and Design Slabs";
            this.btnExtract.Click += new System.EventHandler(this.button1_Click);

            // ── panelInputs (new) ─────────────────────────────────────
            this.panelInputs.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelInputs.Location    = new System.Drawing.Point(670, 12);
            this.panelInputs.Size        = new System.Drawing.Size(300, 200);
            this.panelInputs.Controls.Add(this.lblInputsHdr);
            this.panelInputs.Controls.Add(this.lblFy);
            this.panelInputs.Controls.Add(this.cmbFy);
            this.panelInputs.Controls.Add(this.lblCover);
            this.panelInputs.Controls.Add(this.numCover);
            this.panelInputs.Controls.Add(this.lblBarMain);
            this.panelInputs.Controls.Add(this.numBarMain);
            this.panelInputs.Controls.Add(this.lblBarDist);
            this.panelInputs.Controls.Add(this.numBarDist);
            this.panelInputs.Controls.Add(this.lblFckOverride);
            this.panelInputs.Controls.Add(this.numFckOverride);

            this.lblInputsHdr.Text     = "Design Inputs (apply to all slabs)";
            this.lblInputsHdr.Font     = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblInputsHdr.Location = new System.Drawing.Point(10, 8);
            this.lblInputsHdr.AutoSize = true;

            this.lblFy.Text = "Steel grade fy:";  this.lblFy.Location = new System.Drawing.Point(10, 35); this.lblFy.AutoSize = true;
            this.cmbFy.Location = new System.Drawing.Point(150, 32);
            this.cmbFy.Size = new System.Drawing.Size(130, 22);
            this.cmbFy.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFy.Items.AddRange(new object[] { "Fe250 (250)", "Fe415 (415)", "Fe500 (500)", "Fe550 (550)" });
            this.cmbFy.SelectedIndex = 2;

            this.lblCover.Text   = "Clear cover (mm):"; this.lblCover.Location = new System.Drawing.Point(10, 65); this.lblCover.AutoSize = true;
            this.numCover.Minimum = 15; this.numCover.Maximum = 75; this.numCover.Value = 20;
            this.numCover.Location = new System.Drawing.Point(180, 62); this.numCover.Size = new System.Drawing.Size(100, 22);

            this.lblBarMain.Text = "Bar Ø main (mm):";  this.lblBarMain.Location = new System.Drawing.Point(10, 95); this.lblBarMain.AutoSize = true;
            this.numBarMain.Minimum = 6; this.numBarMain.Maximum = 25; this.numBarMain.Value = 10;
            this.numBarMain.Location = new System.Drawing.Point(180, 92); this.numBarMain.Size = new System.Drawing.Size(100, 22);

            this.lblBarDist.Text = "Bar Ø dist. (mm):";  this.lblBarDist.Location = new System.Drawing.Point(10, 125); this.lblBarDist.AutoSize = true;
            this.numBarDist.Minimum = 6; this.numBarDist.Maximum = 16; this.numBarDist.Value = 8;
            this.numBarDist.Location = new System.Drawing.Point(180, 122); this.numBarDist.Size = new System.Drawing.Size(100, 22);

            this.lblFckOverride.Text = "fck fallback (N/mm²):"; this.lblFckOverride.Location = new System.Drawing.Point(10, 155); this.lblFckOverride.AutoSize = true;
            this.numFckOverride.Minimum = 15; this.numFckOverride.Maximum = 60; this.numFckOverride.Value = 25;
            this.numFckOverride.Location = new System.Drawing.Point(180, 152); this.numFckOverride.Size = new System.Drawing.Size(100, 22);

            // panelProperties (slab details — shifted down to make room)
            this.panelProperties.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelProperties.Controls.Add(this.btnPushToEtabs);
            this.panelProperties.Controls.Add(this.lblStatus);
            this.panelProperties.Controls.Add(this.lblDeflection);
            this.panelProperties.Controls.Add(this.lblThickness);
            this.panelProperties.Controls.Add(this.lblSlabName);
            this.panelProperties.Location = new System.Drawing.Point(670, 220);
            this.panelProperties.Size = new System.Drawing.Size(300, 295);

            this.btnPushToEtabs.Location = new System.Drawing.Point(15, 200);
            this.btnPushToEtabs.Size = new System.Drawing.Size(265, 40);
            this.btnPushToEtabs.Text = "Push New Thickness to ETABS";
            this.btnPushToEtabs.Click += new System.EventHandler(this.btnPushToEtabs_Click);
            this.btnPushToEtabs.Enabled = false;

            this.lblStatus.AutoSize = true; this.lblStatus.Location = new System.Drawing.Point(15, 120); this.lblStatus.Text = "Status: -";
            this.lblDeflection.AutoSize = true; this.lblDeflection.Location = new System.Drawing.Point(15, 90); this.lblDeflection.Text = "Deflection: -";
            this.lblThickness.AutoSize = true; this.lblThickness.Location = new System.Drawing.Point(15, 60); this.lblThickness.Text = "Thickness: -";
            this.lblSlabName.AutoSize = true; this.lblSlabName.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblSlabName.Location = new System.Drawing.Point(15, 15); this.lblSlabName.Text = "Select a Slab";

            // Form1
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 526);
            this.Controls.Add(this.panelInputs);
            this.Controls.Add(this.panelProperties);
            this.Controls.Add(this.btnExtract);
            this.Controls.Add(this.dataGridView1);
            this.Name = "Form1";
            this.Text = "ETABS IS 456 Slab Designer";

            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numCover     )).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBarMain   )).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numBarDist   )).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numFckOverride)).EndInit();
            this.panelProperties.ResumeLayout(false);
            this.panelProperties.PerformLayout();
            this.panelInputs    .ResumeLayout(false);
            this.panelInputs    .PerformLayout();
            this.ResumeLayout(false);
        }
        #endregion

        // Designer fields
        private System.Windows.Forms.DataGridView    dataGridView1   = null!;
        private System.Windows.Forms.Button          btnExtract      = null!;
        private System.Windows.Forms.Panel           panelProperties = null!;
        private System.Windows.Forms.Panel           panelInputs     = null!;
        private System.Windows.Forms.Button          btnPushToEtabs  = null!;
        private System.Windows.Forms.Label           lblStatus       = null!;
        private System.Windows.Forms.Label           lblDeflection   = null!;
        private System.Windows.Forms.Label           lblThickness    = null!;
        private System.Windows.Forms.Label           lblSlabName     = null!;

        // PATCH: new input fields
        private System.Windows.Forms.Label           lblInputsHdr    = null!;
        private System.Windows.Forms.Label           lblFy           = null!;
        private System.Windows.Forms.ComboBox        cmbFy           = null!;
        private System.Windows.Forms.Label           lblCover        = null!;
        private System.Windows.Forms.NumericUpDown   numCover        = null!;
        private System.Windows.Forms.Label           lblBarMain      = null!;
        private System.Windows.Forms.NumericUpDown   numBarMain      = null!;
        private System.Windows.Forms.Label           lblBarDist      = null!;
        private System.Windows.Forms.NumericUpDown   numBarDist      = null!;
        private System.Windows.Forms.Label           lblFckOverride  = null!;
        private System.Windows.Forms.NumericUpDown   numFckOverride  = null!;
    }
}
