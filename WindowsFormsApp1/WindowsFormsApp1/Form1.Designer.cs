namespace WindowsFormsApp1
{
    partial class Form1
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.txtOrigen = new System.Windows.Forms.TextBox();
            this.btnOrigen = new System.Windows.Forms.Button();
            this.btnDestino = new System.Windows.Forms.Button();
            this.txtDestino = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnArchivo = new System.Windows.Forms.Button();
            this.txtArchivo = new System.Windows.Forms.TextBox();
            this.lblArchivo = new System.Windows.Forms.Label();
            this.btnProcesar = new System.Windows.Forms.Button();
            this.listLog = new System.Windows.Forms.ListBox();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.lblContador = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(140, 97);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(224, 30);
            this.label1.TabIndex = 0;
            this.label1.Text = "Ruta donde buscar ";
            // 
            // txtOrigen
            // 
            this.txtOrigen.Location = new System.Drawing.Point(134, 130);
            this.txtOrigen.Name = "txtOrigen";
            this.txtOrigen.Size = new System.Drawing.Size(300, 26);
            this.txtOrigen.TabIndex = 1;
            // 
            // btnOrigen
            // 
            this.btnOrigen.Location = new System.Drawing.Point(507, 123);
            this.btnOrigen.Name = "btnOrigen";
            this.btnOrigen.Size = new System.Drawing.Size(116, 40);
            this.btnOrigen.TabIndex = 2;
            this.btnOrigen.Text = "Buscar";
            this.btnOrigen.UseVisualStyleBackColor = true;
            this.btnOrigen.Click += new System.EventHandler(this.btnOrigen_Click_1);
            // 
            // btnDestino
            // 
            this.btnDestino.Location = new System.Drawing.Point(507, 229);
            this.btnDestino.Name = "btnDestino";
            this.btnDestino.Size = new System.Drawing.Size(116, 40);
            this.btnDestino.TabIndex = 5;
            this.btnDestino.Text = "Buscar";
            this.btnDestino.UseVisualStyleBackColor = true;
            this.btnDestino.Click += new System.EventHandler(this.btnDestino_Click_1);
            // 
            // txtDestino
            // 
            this.txtDestino.Location = new System.Drawing.Point(134, 236);
            this.txtDestino.Name = "txtDestino";
            this.txtDestino.Size = new System.Drawing.Size(300, 26);
            this.txtDestino.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(140, 203);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(228, 30);
            this.label2.TabIndex = 3;
            this.label2.Text = "Ruta donde guardar";
            // 
            // btnArchivo
            // 
            this.btnArchivo.Location = new System.Drawing.Point(507, 347);
            this.btnArchivo.Name = "btnArchivo";
            this.btnArchivo.Size = new System.Drawing.Size(116, 40);
            this.btnArchivo.TabIndex = 8;
            this.btnArchivo.Text = "Buscar";
            this.btnArchivo.UseVisualStyleBackColor = true;
            this.btnArchivo.Click += new System.EventHandler(this.btnArchivo_Click);
            // 
            // txtArchivo
            // 
            this.txtArchivo.Location = new System.Drawing.Point(134, 354);
            this.txtArchivo.Name = "txtArchivo";
            this.txtArchivo.Size = new System.Drawing.Size(300, 26);
            this.txtArchivo.TabIndex = 7;
            // 
            // lblArchivo
            // 
            this.lblArchivo.AutoSize = true;
            this.lblArchivo.Location = new System.Drawing.Point(140, 321);
            this.lblArchivo.Name = "lblArchivo";
            this.lblArchivo.Size = new System.Drawing.Size(413, 30);
            this.lblArchivo.TabIndex = 6;
            this.lblArchivo.Text = "Archivo con documentos a buscar csv";
            // 
            // btnProcesar
            // 
            this.btnProcesar.Location = new System.Drawing.Point(432, 426);
            this.btnProcesar.Name = "btnProcesar";
            this.btnProcesar.Size = new System.Drawing.Size(254, 40);
            this.btnProcesar.TabIndex = 9;
            this.btnProcesar.Text = "Procesar Archivos";
            this.btnProcesar.UseVisualStyleBackColor = true;
            this.btnProcesar.Click += new System.EventHandler(this.btnProcesar_Click_1);
            // 
            // listLog
            // 
            this.listLog.FormattingEnabled = true;
            this.listLog.ItemHeight = 20;
            this.listLog.Location = new System.Drawing.Point(134, 575);
            this.listLog.Name = "listLog";
            this.listLog.Size = new System.Drawing.Size(642, 184);
            this.listLog.TabIndex = 10;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(158, 512);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(559, 36);
            this.progressBar1.TabIndex = 11;
            // 
            // lblContador
            // 
            this.lblContador.AutoSize = true;
            this.lblContador.Location = new System.Drawing.Point(168, 477);
            this.lblContador.Name = "lblContador";
            this.lblContador.Size = new System.Drawing.Size(306, 30);
            this.lblContador.TabIndex = 12;
            this.lblContador.Text = "Documentos procesados: 0";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(963, 811);
            this.Controls.Add(this.lblContador);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.listLog);
            this.Controls.Add(this.btnProcesar);
            this.Controls.Add(this.btnArchivo);
            this.Controls.Add(this.txtArchivo);
            this.Controls.Add(this.lblArchivo);
            this.Controls.Add(this.btnDestino);
            this.Controls.Add(this.txtDestino);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnOrigen);
            this.Controls.Add(this.txtOrigen);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtOrigen;
        private System.Windows.Forms.Button btnOrigen;
        private System.Windows.Forms.Button btnDestino;
        private System.Windows.Forms.TextBox txtDestino;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnArchivo;
        private System.Windows.Forms.TextBox txtArchivo;
        private System.Windows.Forms.Label lblArchivo;
        private System.Windows.Forms.Button btnProcesar;
        private System.Windows.Forms.ListBox listLog;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label lblContador;
    }
}

