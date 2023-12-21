using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace bilgisayarli_görme
{
    public partial class Form1 : Form
    {
        private int secilenCombobox;
        public Form1()
        {
            InitializeComponent();
            comboBox1.Items.Add("Gri Formata Çevir");
            comboBox1.Items.Add("Binary Formata Çevir");
            comboBox1.Items.Add("Görüntünün Histogramını Alma");
            comboBox1.Items.Add("Görüntünün Sobel Kenar Bulma");
            comboBox1.Items.Add("K-Means");

            // Default olarak ilk seçeneği seçelim
            comboBox1.SelectedIndex = 0;
            
        }
        int esik;
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog std = new OpenFileDialog();
            std.ShowDialog();

            pictureBox1.ImageLocation = std.FileName;
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
        }

        private void button2_Click(object sender, EventArgs e)
        {

            string secilenSecenek = comboBox1.SelectedItem.ToString();
            

            switch (secilenSecenek)
            {
                case "Gri Formata Çevir":
                    MessageBox.Show("Gri Formata Çevire tıklandı.");
                    try
                    {
                        Bitmap image = new Bitmap(pictureBox1.Image);
                        Bitmap gri = griYap(image);
                        pictureBox2.Image = gri;
                        pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Fotoğraf Yükleyiniz!");
                    }

                    break;

                case "Binary Formata Çevir":
                    MessageBox.Show("Binary Formata Çevir Tıkladınız");
                    try
                    {
                        Bitmap image = new Bitmap(pictureBox1.Image);
                        Bitmap binary = binaryYap(image);
                        pictureBox2.Image = binary;
                        pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Fotoğraf Yükleyiniz!");
                    }
                    break;

                case "Görüntünün Histogramını Alma":
                    MessageBox.Show("Görüntünün Histogramını Almaya Tıkladınız");
                    Bitmap bmp = new Bitmap(pictureBox2.Image);
                    // RGB renk kanalları için histogramlar
                    int[] histogramR = new int[256];
                    int[] histogramG = new int[256];
                    int[] histogramB = new int[256];

                    // Her pikseldeki renk bilgilerini histograma ekle
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        for (int x = 0; x < bmp.Width; x++)
                        {
                            Color color = bmp.GetPixel(x, y);
                            histogramR[color.R]++;
                            histogramG[color.G]++;
                            histogramB[color.B]++;
                        }
                    }

                    // Histogram grafiğini oluştur
                    chart1.Series.Clear();

                    Series seriesR = new Series("Red");
                    seriesR.Color = Color.Red;

                    Series seriesG = new Series("Green");
                    seriesG.Color = Color.Green;

                    Series seriesB = new Series("Blue");
                    seriesB.Color = Color.Blue;

                    for (int i = 0; i < 256; i++)
                    {
                        seriesR.Points.AddXY(i, histogramR[i]);
                        seriesG.Points.AddXY(i, histogramG[i]);
                        seriesB.Points.AddXY(i, histogramB[i]);
                    }

                    chart1.Series.Add(seriesR);
                    chart1.Series.Add(seriesG);
                    chart1.Series.Add(seriesB);
                    chart1.Visible = true;

                    break;
                case "Görüntünün Sobel Kenar Bulma":
                    MessageBox.Show("Görüntünün Sobel Kenar Bulmaya tıklandı.");
                    Bitmap image2 = new Bitmap(pictureBox1.Image);
                    Bitmap sobel = sobelYap(image2);
                    pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
                    pictureBox2.Image = sobel;

                    break;
                case "K-Means":
                    MessageBox.Show("Girdiğiniz Değere göre Görüntünün K-Means Segmentasyonu yapıldı");
                    pictureBox2.Image = KMeansSegmentasyon(pictureBox2.Image,secilenCombobox);


                    break;

                default:
                    // Beklenmeyen bir durumu ele alabilirsiniz.
                    break;
            }

           
        }

        private Bitmap griYap(Bitmap bmp)
        {
            for (int i = 0; i < bmp.Height - 1; i++)
            {
                for (int j = 0; j < bmp.Width - 1; j++)
                {
                    int deger = (bmp.GetPixel(j, i).R + bmp.GetPixel(j, i).G + bmp.GetPixel(j, i).B) / 3;
                    Color renk = Color.FromArgb(deger, deger, deger);

                    bmp.SetPixel(j, i, renk);
                }
            }
            return bmp;
        }

        private Bitmap binaryYap(Bitmap bmp)
        {
            int tmp = 0;
            Bitmap gri = griYap(bmp);
            int esik = esikBul(gri);
            Color renk;
            for (int i = 0; i < gri.Height - 1; i++)
            {
                for (int j = 0; j < gri.Width - 1; j++)
                {
                    tmp = gri.GetPixel(j, i).G;
                    if (tmp < esik)
                    {
                        renk = Color.FromArgb(0, 0, 0);
                        gri.SetPixel(j, i, renk);
                    }
                    else
                    {
                        renk = Color.FromArgb(255, 255, 255);
                        gri.SetPixel(j, i, renk);
                    }
                }
                
            }
            return gri;
        }

        private Bitmap sobelYap(Bitmap image)
        {
            Bitmap gri = griYap(image);
            Bitmap buffer = new Bitmap(gri.Width, gri.Height);//görüntünün boyutlarına sahip boş görüntü oluşturuyorsun
            Color renk;
            int valx, valy, gradient;
            int[,] GX = new int[3, 3];
            int[,] GY = new int[3, 3];
            //Yatay kenar 
            GX[0, 0] = -1; GX[0, 1] = 0; GX[0, 2] = 1;
            GX[1, 0] = -2; GX[1, 1] = 0; GX[1, 2] = 2;
            GX[2, 0] = -1; GX[2, 1] = 0; GX[2, 2] = 1;


            //Dikey kenar

            GY[0, 0] = -1; GY[0, 1] = -2; GY[0, 2] = -1;
            GY[1, 0] = 0; GY[1, 1] = 0; GY[1, 2] = 0;
            GY[2, 0] = 1; GY[2, 1] = 2; GY[2, 2] = 1;


            for (int i = 0; i < image.Height; i++)
            {
                for (int j = 0; j < image.Width; j++)
                {
                    if (i == 0 || i == gri.Height - 1 || j == 0 || j == gri.Width - 1)
                    {
                        renk = Color.FromArgb(255, 255, 255);
                        buffer.SetPixel(j, i, renk);
                        valx = 0;
                        valy = 0;
                    }
                    else
                    {
                        valx = gri.GetPixel(j - 1, i - 1).R * GX[0, 0]
                            + gri.GetPixel(j, i - 1).R * GX[0, 1]
                            + gri.GetPixel(j + 1, i - 1).R * GX[0, 2]
                            + gri.GetPixel(j - 1, i).R * GX[1, 0]
                            + gri.GetPixel(j, i).R * GX[1, 1]
                            + gri.GetPixel(j + 1, i).R * GX[1, 2]
                            + gri.GetPixel(j - 1, i + 1).R * GX[2, 0]
                            + gri.GetPixel(j, i + 1).R * GX[2, 1]
                            + gri.GetPixel(j + 1, i + 1).R * GX[2, 2];

                        valy = gri.GetPixel(j - 1, i - 1).R * GY[0, 0]
                             + gri.GetPixel(j, i - 1).R * GY[0, 1]
                             + gri.GetPixel(j + 1, i - 1).R * GY[0, 2]
                             + gri.GetPixel(j - 1, i).R * GY[1, 0]
                             + gri.GetPixel(j, i).R * GY[1, 1]
                             + gri.GetPixel(j + 1, i).R * GY[1, 2]
                             + gri.GetPixel(j - 1, i + 1).R * GY[2, 0]
                             + gri.GetPixel(j, i + 1).R * GY[2, 1]
                             + gri.GetPixel(j + 1, i + 1).R * GY[2, 2];

                        gradient = (int)(Math.Abs(valx) + Math.Abs(valy));


                        if (gradient < 0)
                            gradient = 0;
                        if (gradient > 255)
                            gradient = 255;

                        renk = Color.FromArgb(gradient, gradient, gradient);
                        buffer.SetPixel(j, i, renk);


                    }
                }
            }
            return buffer; ;
        }

        int esikBul(Bitmap gri)
        {
                int enb = gri.GetPixel(0, 0).G;
                int enk = gri.GetPixel(0, 0).G;
                for (int i = 0; i < gri.Height - 1; i++)
                {
                    for (int j = 0; j < gri.Width - 1; j++)
                    {
                        if (enb > gri.GetPixel(j, i).G)
                            enb = gri.GetPixel(j, i).G;
                        if (enk < gri.GetPixel(j, i).G)
                            enk = gri.GetPixel(j, i).G;

                    }
                }
                int a = enb;
                int b = enk;
                esik = (a + b) / 2;
                return esik;
        }
        private Bitmap KMeansSegmentasyon(Image image, int deger)
        {
           

            Bitmap bmp = new Bitmap(image);
            Random random = new Random();

            // K-Means parametreleri
            int k = deger; // Kaç grup olacağı
            int iterasyonSayisi = 10; // İterasyon sayısı

            // Rastgele başlangıç merkezleri oluştur
            List<Color> merkezRenkler = new List<Color>();
            for (int i = 0; i < k; i++)
            {
                int x = random.Next(0, bmp.Width);
                int y = random.Next(0, bmp.Height);
                merkezRenkler.Add(bmp.GetPixel(x, y));
            }

            // K-Means iterasyonları
            for (int iterasyon = 0; iterasyon < iterasyonSayisi; iterasyon++)
            {
                // Her pikseli en yakın merkeze atayarak grupla
                List<List<Color>> gruplar = new List<List<Color>>();
                for (int i = 0; i < k; i++)
                {
                    gruplar.Add(new List<Color>());
                }

                for (int y = 0; y < bmp.Height; y++)
                {
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        Color pixelRenk = bmp.GetPixel(x, y);

                        // En yakın merkezi bul
                        int enYakinIndex = 0;
                        double enKucukMesafe = double.MaxValue;
                        for (int i = 0; i < k; i++)
                        {
                            double mesafe = RenkMesafe(pixelRenk, merkezRenkler[i]);
                            if (mesafe < enKucukMesafe)
                            {
                                enYakinIndex = i;
                                enKucukMesafe = mesafe;
                            }
                        }

                        // Pikseli grupla
                        gruplar[enYakinIndex].Add(pixelRenk);
                    }
                }

                // Grupların merkezlerini güncelle
                for (int i = 0; i < k; i++)
                {
                    if (gruplar[i].Count > 0)
                    {
                        Color yeniMerkez = GrupOrtalamaRenk(gruplar[i]);
                        merkezRenkler[i] = yeniMerkez;
                    }
                }
            }

            // Yeni bir Bitmap oluştur ve pikselleri gruplara göre renklendir
            Bitmap segmentasyonSonuc = new Bitmap(bmp.Width, bmp.Height);
            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    Color pixelRenk = bmp.GetPixel(x, y);

                    // En yakın merkezi bul
                    int enYakinIndex = 0;
                    double enKucukMesafe = double.MaxValue;
                    for (int i = 0; i < k; i++)
                    {
                        double mesafe = RenkMesafe(pixelRenk, merkezRenkler[i]);
                        if (mesafe < enKucukMesafe)
                        {
                            enYakinIndex = i;
                            enKucukMesafe = mesafe;
                        }
                    }

                    // Pikseli en yakın merkezin rengine ayarla
                    segmentasyonSonuc.SetPixel(x, y, merkezRenkler[enYakinIndex]);
                }
            }

            return segmentasyonSonuc;
        }
        private double RenkMesafe(Color renk1, Color renk2)
        {
            // RGB renkler arasındaki mesafeyi hesapla
            double rMesafe = Math.Pow(renk1.R - renk2.R, 2);
            double gMesafe = Math.Pow(renk1.G - renk2.G, 2);
            double bMesafe = Math.Pow(renk1.B - renk2.B, 2);

            return Math.Sqrt(rMesafe + gMesafe + bMesafe);
        }
        private Color GrupOrtalamaRenk(List<Color> grup)
        {
            // Grubun ortalama rengini hesapla
            int toplamR = 0;
            int toplamG = 0;
            int toplamB = 0;

            foreach (Color renk in grup)
            {
                toplamR += renk.R;
                toplamG += renk.G;
                toplamB += renk.B;
            }

            int ortalamaR = toplamR / grup.Count;
            int ortalamaG = toplamG / grup.Count;
            int ortalamaB = toplamB / grup.Count;

            return Color.FromArgb(ortalamaR, ortalamaG, ortalamaB);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            chart1.Visible = false;
            for (int i = 1; i <= 50; i++)
            {
                // ComboBox'a değeri ekleyin
                comboBox2.Items.Add(i);
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.SelectedItem != null)
            {
                secilenCombobox = Convert.ToInt32(comboBox2.SelectedItem);

            }
        }
    }
}
    
   

