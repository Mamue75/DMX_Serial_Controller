using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DMX_Serial_Controller.Properties;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;

namespace DMX_Serial_Controller
{
    public partial class Form1 : Form
    {

        int[] dmx_ch = new int[512];
        int[] scene1 = new int[512];
        int[] scene2 = new int[512];
        int[] scene3 = new int[512];
        int[] scene4 = new int[512];
        int[] scene5 = new int[512];
        int[] scene6 = new int[512];


        int seq = 1;
        int[] numSeq = new int[6];
        bool seqEnable = false;

        int panelSelected = 1;

        public Form1()
        {
            InitializeComponent();
        }



        private void updateCOMPortsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                PortBox.Items.Clear();
                PortBox.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());
                PortBox.SelectedIndex = 0;
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            tabControl1.Enabled = false;
            

            try
            {
                PortBox.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());
                PortBox.SelectedIndex = 0;
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
      
            string value = Settings.Default["dmx_scene_save"].ToString();
            String[] strArray = value.Split('x');

            scene1 = strArray[0].Split(',').Select(s => int.Parse(s)).ToArray();
            scene2 = strArray[1].Split(',').Select(s => int.Parse(s)).ToArray();
            scene3 = strArray[2].Split(',').Select(s => int.Parse(s)).ToArray();
            scene4 = strArray[3].Split(',').Select(s => int.Parse(s)).ToArray();
            scene5 = strArray[4].Split(',').Select(s => int.Parse(s)).ToArray();
            scene6 = strArray[5].Split(',').Select(s => int.Parse(s)).ToArray();

            /*
            string result1 = string.Join(",", scene1);
            MessageBox.Show(result1);
            string result5 = string.Join(",", scene5);
            MessageBox.Show(result5);
            */

        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (labConnected.Text != "connected")
            {
                serialPort1.PortName = PortBox.Text;
                try
                {
                    serialPort1.Open();
                    serialPort1.Write("z");
                }
                catch (Exception error)
                {
                    MessageBox.Show(error.Message);
                }
                labConnected.ForeColor = Color.ForestGreen;
                labConnected.Text = "CONNECTED";
                //labConnected.Text = serialPort1.PortName;
                
                updateCOMPortsToolStripMenuItem.Enabled = false;
                
                tabControl1.Enabled = true;
                groupBox2.Enabled = true;
                groupBox3.Enabled = true;
                groupBox4.Enabled = true;

                btnZero.Enabled = true;
                updateChannelValues();

            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            zeroChannels();
            serialPort1.Close();
            labConnected.ForeColor = Color.Red;
            labConnected.Text = "DISCONNECTED";
            updateCOMPortsToolStripMenuItem.Enabled = true;

            tabControl1.Enabled = false;
            groupBox2.Enabled = false;
            groupBox3.Enabled = false;
            groupBox4.Enabled = false;

            btnZero.Enabled = false;
            
        }        
        
        
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (serialPort1.IsOpen) { 
                try
                {
                    zeroChannels();
                    serialPort1.Close();
                }
                catch (Exception error)
                {
                    MessageBox.Show(error.Message);
                }
            }

            try
            {
                if(!string.Equals(scene1, Settings.Default["dmx_scene_save"]))
                {
                    scene1[0] = 1;
                    scene2[0] = 2;
                    scene3[0] = 3;
                    scene4[0] = 4;
                    scene5[0] = 5;
                    scene6[0] = 6;
                    string dmxScene1 = String.Join(",", scene1.Select(i => i.ToString()).ToArray());
                    string dmxScene2 = String.Join(",", scene2.Select(i => i.ToString()).ToArray());
                    string dmxScene3 = String.Join(",", scene3.Select(i => i.ToString()).ToArray());
                    string dmxScene4 = String.Join(",", scene4.Select(i => i.ToString()).ToArray());
                    string dmxScene5 = String.Join(",", scene5.Select(i => i.ToString()).ToArray());
                    string dmxScene6 = String.Join(",", scene6.Select(i => i.ToString()).ToArray());

                    string dmxScenesComp = dmxScene1 + "x" + dmxScene2 + "x" + dmxScene3 + "x" + dmxScene4 + "x" + dmxScene5 + "x" + dmxScene6;
                    //MessageBox.Show(dmxScenesComp);
                    Settings.Default["dmx_scene_save"] = dmxScenesComp;
                    Settings.Default.Save();

                }
                
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
            

        }


        private void btnZero_Click(object sender, EventArgs e)
        {
            
            zeroChannels();
            

        }

        void zeroChannels()
        {
            serialPort1.Write("z");
            for (int i = 1; i < dmx_ch.Length; i++)
            {
                dmx_ch[i] = 0;
            }
            updateChannelValues();
            updatePanel();
        }


        void updateChannelValues()
        {
            string channels = "";


            for (int i = 1; i <= 128; i++)
            {
                string num = i.ToString();
                
                if (i < 100)
                {
                    num = "0" + num;
                }

                if (i < 10)
                {
                    num = "0" + num;
                }

                string space = "";

                if (dmx_ch[i] < 100)
                {
                    space = "x";
                }

                if (dmx_ch[i] < 10)
                {
                    space = "xx";
                }

                channels = channels + "CH" + num + " = " + dmx_ch[i].ToString() + space + "   |   ";

                if (i % 16 == 0)
                {
                    channels = channels + "\n\n";
                }

            }
            labValDMX.Text = channels;  

      
        }


        private void panelSelector_ValueChanged(object sender, EventArgs e)
        {
            panelSelected = Convert.ToInt16(panelSelector.Value);
            updatePanel();
        }


        void updatePanel()
        {
            groupBoxCH1.Text = "CHANNEL " + (((panelSelected - 1) * 24) + 1);
            groupBoxCH2.Text = "CHANNEL " + (((panelSelected - 1) * 24) + 2);
            groupBoxCH3.Text = "CHANNEL " + (((panelSelected - 1) * 24) + 3);
            groupBoxCH4.Text = "CHANNEL " + (((panelSelected - 1) * 24) + 4);
            groupBoxCH5.Text = "CHANNEL " + (((panelSelected - 1) * 24) + 5);
            groupBoxCH6.Text = "CHANNEL " + (((panelSelected - 1) * 24) + 6);
            groupBoxCH7.Text = "CHANNEL " + (((panelSelected - 1) * 24) + 7);
            groupBoxCH8.Text = "CHANNEL " + (((panelSelected - 1) * 24) + 8);
            groupBoxCH9.Text = "CHANNEL " + (((panelSelected - 1) * 24) + 9);
            groupBoxCH10.Text = "CHANNEL " + (((panelSelected - 1) * 24) + 10);
            groupBoxCH11.Text = "CHANNEL " + (((panelSelected - 1) * 24) + 11);
            groupBoxCH12.Text = "CHANNEL " + (((panelSelected - 1) * 24) + 12);

            groupBoxCH13.Text = "CHANNEL " + (((panelSelected - 1) * 24) + 13);
            groupBoxCH14.Text = "CHANNEL " + (((panelSelected - 1) * 24) + 14);
            groupBoxCH15.Text = "CHANNEL " + (((panelSelected - 1) * 24) + 15);
            groupBoxCH16.Text = "CHANNEL " + (((panelSelected - 1) * 24) + 16);
            groupBoxCH17.Text = "CHANNEL " + (((panelSelected - 1) * 24) + 17);
            groupBoxCH18.Text = "CHANNEL " + (((panelSelected - 1) * 24) + 18);
            groupBoxCH19.Text = "CHANNEL " + (((panelSelected - 1) * 24) + 19);
            groupBoxCH20.Text = "CHANNEL " + (((panelSelected - 1) * 24) + 20);
            groupBoxCH21.Text = "CHANNEL " + (((panelSelected - 1) * 24) + 21);
            groupBoxCH22.Text = "CHANNEL " + (((panelSelected - 1) * 24) + 22);
            groupBoxCH23.Text = "CHANNEL " + (((panelSelected - 1) * 24) + 23);
            groupBoxCH24.Text = "CHANNEL " + (((panelSelected - 1) * 24) + 24);


            trackBar1.Value = dmx_ch[(((panelSelected - 1) * 24) + 1)];
            labValue1.Text = dmx_ch[(((panelSelected - 1) * 24) + 1)].ToString();

            trackBar2.Value = dmx_ch[(((panelSelected - 1) * 24) + 2)];
            labValue2.Text = dmx_ch[(((panelSelected - 1) * 24) + 2)].ToString();

            trackBar3.Value = dmx_ch[(((panelSelected - 1) * 24) + 3)];
            labValue3.Text = dmx_ch[(((panelSelected - 1) * 24) + 3)].ToString();

            trackBar4.Value = dmx_ch[(((panelSelected - 1) * 24) + 4)];
            labValue4.Text = dmx_ch[(((panelSelected - 1) * 24) + 4)].ToString();

            trackBar5.Value = dmx_ch[(((panelSelected - 1) * 24) + 5)];
            labValue5.Text = dmx_ch[(((panelSelected - 1) * 24) + 5)].ToString();

            trackBar6.Value = dmx_ch[(((panelSelected - 1) * 24) + 6)];
            labValue6.Text = dmx_ch[(((panelSelected - 1) * 24) + 6)].ToString();

            trackBar7.Value = dmx_ch[(((panelSelected - 1) * 24) + 7)];
            labValue7.Text = dmx_ch[(((panelSelected - 1) * 24) + 7)].ToString();

            trackBar8.Value = dmx_ch[(((panelSelected - 1) * 24) + 8)];
            labValue8.Text = dmx_ch[(((panelSelected - 1) * 24) + 8)].ToString();

            trackBar9.Value = dmx_ch[(((panelSelected - 1) * 24) + 9)];
            labValue9.Text = dmx_ch[(((panelSelected - 1) * 24) + 9)].ToString();

            trackBar10.Value = dmx_ch[(((panelSelected - 1) * 24) + 10)];
            labValue10.Text = dmx_ch[(((panelSelected - 1) * 24) + 10)].ToString();

            trackBar11.Value = dmx_ch[(((panelSelected - 1) * 24) + 11)];
            labValue11.Text = dmx_ch[(((panelSelected - 1) * 24) + 11)].ToString();

            trackBar12.Value = dmx_ch[(((panelSelected - 1) * 24) + 12)];
            labValue12.Text = dmx_ch[(((panelSelected - 1) * 24) + 12)].ToString();



            trackBar13.Value = dmx_ch[(((panelSelected - 1) * 24) + 13)];
            labValue13.Text = dmx_ch[(((panelSelected - 1) * 24) + 13)].ToString();

            trackBar14.Value = dmx_ch[(((panelSelected - 1) * 24) + 14)];
            labValue14.Text = dmx_ch[(((panelSelected - 1) * 24) + 14)].ToString();

            trackBar15.Value = dmx_ch[(((panelSelected - 1) * 24) + 15)];
            labValue15.Text = dmx_ch[(((panelSelected - 1) * 24) + 15)].ToString();

            trackBar16.Value = dmx_ch[(((panelSelected - 1) * 24) + 16)];
            labValue16.Text = dmx_ch[(((panelSelected - 1) * 24) + 16)].ToString();

            trackBar17.Value = dmx_ch[(((panelSelected - 1) * 24) + 17)];
            labValue17.Text = dmx_ch[(((panelSelected - 1) * 24) + 17)].ToString();

            trackBar18.Value = dmx_ch[(((panelSelected - 1) * 24) + 18)];
            labValue18.Text = dmx_ch[(((panelSelected - 1) * 24) + 18)].ToString();

            trackBar19.Value = dmx_ch[(((panelSelected - 1) * 24) + 19)];
            labValue19.Text = dmx_ch[(((panelSelected - 1) * 24) + 19)].ToString();

            trackBar20.Value = dmx_ch[(((panelSelected - 1) * 24) + 20)];
            labValue20.Text = dmx_ch[(((panelSelected - 1) * 24) + 20)].ToString();

            trackBar21.Value = dmx_ch[(((panelSelected - 1) * 24) + 21)];
            labValue21.Text = dmx_ch[(((panelSelected - 1) * 24) + 21)].ToString();

            trackBar22.Value = dmx_ch[(((panelSelected - 1) * 24) + 22)];
            labValue22.Text = dmx_ch[(((panelSelected - 1) * 24) + 22)].ToString();

            trackBar23.Value = dmx_ch[(((panelSelected - 1) * 24) + 23)];
            labValue23.Text = dmx_ch[(((panelSelected - 1) * 24) + 23)].ToString();

            trackBar24.Value = dmx_ch[(((panelSelected - 1) * 24) + 24)];
            labValue24.Text = dmx_ch[(((panelSelected - 1) * 24) + 24)].ToString();


        }

        void dmx_write(int channel, int value)
        {
            try
            {
                serialPort1.Write((((panelSelected - 1) * 24) + channel) + "c" + value + "w");
                dmx_ch[(((panelSelected - 1) * 24) + channel)] = value;
                
                updateChannelValues();
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
        }

        void dmx_write2(int channel, int value)
        {
            try
            {
                serialPort1.Write(channel + "c" + value + "w");
                dmx_ch[channel] = value;

                updateChannelValues();
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
        }


        private void btnSavePreset_Click(object sender, EventArgs e)
        {
            switch (presetSelect.Value)
            {
                case 1: for (int i = 1; i < scene1.Length; i++)
                    {
                        scene1[i] = dmx_ch[i];
                    }
                        break;
                case 2:
                    for (int i = 1; i < scene2.Length; i++)
                    {
                        scene2[i] = dmx_ch[i];
                    }
                    break;
                case 3:
                    for (int i = 1; i < scene3.Length; i++)
                    {
                        scene3[i] = dmx_ch[i];
                    }
                    break;
                case 4:
                    for (int i = 1; i < scene4.Length; i++)
                    {
                        scene4[i] = dmx_ch[i];
                    }
                    break;
                case 5:
                    for (int i = 1; i < scene5.Length; i++)
                    {
                        scene5[i] = dmx_ch[i];
                    }
                    break;
                case 6:
                    for (int i = 1; i < scene6.Length; i++)
                    {
                        scene6[i] = dmx_ch[i];
                    }
                    break;
            }
            MessageBox.Show("SCENE " + presetSelect.Value.ToString() + " sucessfully saved!");
            

        }


        private void btnS1_Click(object sender, EventArgs e)
        {
            for(int i = 1; i < scene1.Length; i++)
            {
                if(dmx_ch[i] != scene1[i])
                {
                    dmx_ch[i] = scene1[i];
                    dmx_write2(i, dmx_ch[i]);
                }
            }
            updateChannelValues();
            updatePanel();

        }

        private void btnS2_Click(object sender, EventArgs e)
        {
            for (int i = 1; i < scene2.Length; i++)
            {
                if (dmx_ch[i] != scene2[i])
                {
                    dmx_ch[i] = scene2[i];
                    dmx_write2(i, dmx_ch[i]);
                }
            }
            updateChannelValues();
            updatePanel();
        }


        private void btnS3_Click(object sender, EventArgs e)
        {
            for (int i = 1; i < scene3.Length; i++)
            {
                if (dmx_ch[i] != scene3[i])
                {
                    dmx_ch[i] = scene3[i];
                    dmx_write2(i, dmx_ch[i]);
                }
            }
            updateChannelValues();
            updatePanel();
        }

        private void btnS4_Click(object sender, EventArgs e)
        {
            for (int i = 1; i < scene4.Length; i++)
            {
                if (dmx_ch[i] != scene4[i])
                {
                    dmx_ch[i] = scene4[i];
                    dmx_write2(i, dmx_ch[i]);
                }
            }
            updateChannelValues();
            updatePanel();
        }

        private void btnS5_Click(object sender, EventArgs e)
        {
            for (int i = 1; i < scene5.Length; i++)
            {
                if (dmx_ch[i] != scene5[i])
                {
                    dmx_ch[i] = scene5[i];
                    dmx_write2(i, dmx_ch[i]);
                }
            }
            updateChannelValues();
            updatePanel();
        }

        private void btnS6_Click(object sender, EventArgs e)
        {
            for (int i = 1; i < scene6.Length; i++)
            {
                if (dmx_ch[i] != scene6[i])
                {
                    dmx_ch[i] = scene6[i];
                    dmx_write2(i, dmx_ch[i]);
                }
            }
            updateChannelValues();
            updatePanel();
        }


        //____________________________________________________________________________________________________________



        private void btnOn1_Click_1(object sender, EventArgs e)
        {
            dmx_write(1, 255);
            trackBar1.Value = dmx_ch[(((panelSelected - 1) * 24) + 1)];
            labValue1.Text = dmx_ch[(((panelSelected - 1) * 24) + 1)].ToString();
        }

        private void btnOn2_Click_1(object sender, EventArgs e)
        {
            dmx_write(2, 255);
            trackBar2.Value = dmx_ch[(((panelSelected - 1) * 24) + 2)];
            labValue2.Text = dmx_ch[(((panelSelected - 1) * 24) + 2)].ToString();
        }

        private void btnOn3_Click_1(object sender, EventArgs e)
        {
            dmx_write(3, 255);
            trackBar3.Value = dmx_ch[(((panelSelected - 1) * 24) + 3)];
            labValue3.Text = dmx_ch[(((panelSelected - 1) * 24) + 3)].ToString();
        }

        private void btnOn4_Click(object sender, EventArgs e)
        {
            dmx_write(4, 255);
            trackBar4.Value = dmx_ch[(((panelSelected - 1) * 24) + 4)];
            labValue4.Text = dmx_ch[(((panelSelected - 1) * 24) + 4)].ToString();
        }

        private void btnOn5_Click(object sender, EventArgs e)
        {
            dmx_write(5, 255);
            trackBar5.Value = dmx_ch[(((panelSelected - 1) * 24) + 5)];
            labValue5.Text = dmx_ch[(((panelSelected - 1) * 24) + 5)].ToString();
        }

        private void btnOn6_Click(object sender, EventArgs e)
        {
            dmx_write(6, 255);
            trackBar6.Value = dmx_ch[(((panelSelected - 1) * 24) + 6)];
            labValue6.Text = dmx_ch[(((panelSelected - 1) * 24) + 6)].ToString();
        }

        private void btnOn7_Click(object sender, EventArgs e)
        {
            dmx_write(7, 255);
            trackBar7.Value = dmx_ch[(((panelSelected - 1) * 24) + 7)];
            labValue7.Text = dmx_ch[(((panelSelected - 1) * 24) + 7)].ToString();
        }

        private void btnOn8_Click(object sender, EventArgs e)
        {
            dmx_write(8, 255);
            trackBar8.Value = dmx_ch[(((panelSelected - 1) * 24) + 8)];
            labValue8.Text = dmx_ch[(((panelSelected - 1) * 24) + 8)].ToString();
        }

        private void btnOn9_Click(object sender, EventArgs e)
        {
            dmx_write(9, 255);
            trackBar9.Value = dmx_ch[(((panelSelected - 1) * 24) + 9)];
            labValue9.Text = dmx_ch[(((panelSelected - 1) * 24) + 9)].ToString();
        }

        private void btnOn10_Click(object sender, EventArgs e)
        {
            dmx_write(10, 255);
            trackBar10.Value = dmx_ch[(((panelSelected - 1) * 24) + 10)];
            labValue10.Text = dmx_ch[(((panelSelected - 1) * 24) + 10)].ToString();
        }

        private void btnOn11_Click(object sender, EventArgs e)
        {
            dmx_write(11, 255);
            trackBar11.Value = dmx_ch[(((panelSelected - 1) * 24) + 11)];
            labValue11.Text = dmx_ch[(((panelSelected - 1) * 24) + 11)].ToString();
        }

        private void btnOn12_Click(object sender, EventArgs e)
        {
            dmx_write(12, 255);
            trackBar12.Value = dmx_ch[(((panelSelected - 1) * 24) + 12)];
            labValue12.Text = dmx_ch[(((panelSelected - 1) * 24) + 12)].ToString();
        }

        private void btnOn13_Click(object sender, EventArgs e)
        {
            dmx_write(13, 255);
            trackBar13.Value = dmx_ch[(((panelSelected - 1) * 24) + 13)];
            labValue13.Text = dmx_ch[(((panelSelected - 1) * 24) + 13)].ToString();
        }

        private void btnOn14_Click(object sender, EventArgs e)
        {
            dmx_write(14, 255);
            trackBar14.Value = dmx_ch[(((panelSelected - 1) * 24) + 14)];
            labValue14.Text = dmx_ch[(((panelSelected - 1) * 24) + 14)].ToString();
        }

        private void btnOn15_Click(object sender, EventArgs e)
        {
            dmx_write(15, 255);
            trackBar15.Value = dmx_ch[(((panelSelected - 1) * 24) + 15)];
            labValue15.Text = dmx_ch[(((panelSelected - 1) * 24) + 15)].ToString();
        }

        private void btnOn16_Click(object sender, EventArgs e)
        {
            dmx_write(16, 255);
            trackBar16.Value = dmx_ch[(((panelSelected - 1) * 24) + 16)];
            labValue16.Text = dmx_ch[(((panelSelected - 1) * 24) + 16)].ToString();
        }

        private void btnOn17_Click(object sender, EventArgs e)
        {
            dmx_write(17, 255);
            trackBar17.Value = dmx_ch[(((panelSelected - 1) * 24) + 17)];
            labValue17.Text = dmx_ch[(((panelSelected - 1) * 24) + 17)].ToString();
        }

        private void btnOn18_Click(object sender, EventArgs e)
        {
            dmx_write(18, 255);
            trackBar18.Value = dmx_ch[(((panelSelected - 1) * 24) + 18)];
            labValue18.Text = dmx_ch[(((panelSelected - 1) * 24) + 18)].ToString();
        }

        private void btnOn19_Click(object sender, EventArgs e)
        {
            dmx_write(19, 255);
            trackBar19.Value = dmx_ch[(((panelSelected - 1) * 24) + 19)];
            labValue19.Text = dmx_ch[(((panelSelected - 1) * 24) + 19)].ToString();
        }

        private void btnOn20_Click(object sender, EventArgs e)
        {
            dmx_write(20, 255);
            trackBar20.Value = dmx_ch[(((panelSelected - 1) * 24) + 20)];
            labValue20.Text = dmx_ch[(((panelSelected - 1) * 24) + 20)].ToString();
        }

        private void btnOn21_Click(object sender, EventArgs e)
        {
            dmx_write(21, 255);
            trackBar21.Value = dmx_ch[(((panelSelected - 1) * 24) + 21)];
            labValue21.Text = dmx_ch[(((panelSelected - 1) * 24) + 21)].ToString();
        }

        private void btnOn22_Click(object sender, EventArgs e)
        {
            dmx_write(22, 255);
            trackBar22.Value = dmx_ch[(((panelSelected - 1) * 24) + 22)];
            labValue22.Text = dmx_ch[(((panelSelected - 1) * 24) + 22)].ToString();
        }

        private void btnOn23_Click(object sender, EventArgs e)
        {
            dmx_write(23, 255);
            trackBar23.Value = dmx_ch[(((panelSelected - 1) * 24) + 23)];
            labValue23.Text = dmx_ch[(((panelSelected - 1) * 24) + 23)].ToString();
        }

        private void btnOn24_Click(object sender, EventArgs e)
        {
            dmx_write(24, 255);
            trackBar24.Value = dmx_ch[(((panelSelected - 1) * 24) + 24)];
            labValue24.Text = dmx_ch[(((panelSelected - 1) * 24) + 24)].ToString();
        }



        //____________________________________________________________________________________________________________




        private void btnOff1_Click_1(object sender, EventArgs e)
        {
            dmx_write(1, 0);
            trackBar1.Value = dmx_ch[(((panelSelected - 1) * 24) + 1)];
            labValue1.Text = dmx_ch[(((panelSelected - 1) * 24) + 1)].ToString();
        }

        private void btnOff2_Click_1(object sender, EventArgs e)
        {
            dmx_write(2, 0);
            trackBar2.Value = dmx_ch[(((panelSelected - 1) * 24) + 2)];
            labValue2.Text = dmx_ch[(((panelSelected - 1) * 24) + 2)].ToString();
        }

        private void btnOff3_Click_1(object sender, EventArgs e)
        {
            dmx_write(3, 0);
            trackBar3.Value = dmx_ch[(((panelSelected - 1) * 24) + 3)];
            labValue3.Text = dmx_ch[(((panelSelected - 1) * 24) + 3)].ToString();
        }

        private void btnOff4_Click(object sender, EventArgs e)
        {
            dmx_write(4, 0);
            trackBar4.Value = dmx_ch[(((panelSelected - 1) * 24) + 4)];
            labValue4.Text = dmx_ch[(((panelSelected - 1) * 24) + 4)].ToString();
        }

        private void btnOff5_Click(object sender, EventArgs e)
        {
            dmx_write(5, 0);
            trackBar5.Value = dmx_ch[(((panelSelected - 1) * 24) + 5)];
            labValue5.Text = dmx_ch[(((panelSelected - 1) * 24) + 5)].ToString();
        }

        private void btnOff6_Click(object sender, EventArgs e)
        {
            dmx_write(6, 0);
            trackBar6.Value = dmx_ch[(((panelSelected - 1) * 24) + 6)];
            labValue6.Text = dmx_ch[(((panelSelected - 1) * 24) + 6)].ToString();
        }

        private void btnOff7_Click(object sender, EventArgs e)
        {
            dmx_write(7, 0);
            trackBar7.Value = dmx_ch[(((panelSelected - 1) * 24) + 7)];
            labValue7.Text = dmx_ch[(((panelSelected - 1) * 24) + 7)].ToString();
        }

        private void btnOff8_Click(object sender, EventArgs e)
        {
            dmx_write(8, 0);
            trackBar8.Value = dmx_ch[(((panelSelected - 1) * 24) + 8)];
            labValue8.Text = dmx_ch[(((panelSelected - 1) * 24) + 8)].ToString();
        }

        private void btnOff9_Click(object sender, EventArgs e)
        {
            dmx_write(9, 0);
            trackBar9.Value = dmx_ch[(((panelSelected - 1) * 24) + 9)];
            labValue9.Text = dmx_ch[(((panelSelected - 1) * 24) + 9)].ToString();
        }

        private void btnOff10_Click(object sender, EventArgs e)
        {
            dmx_write(10, 0);
            trackBar10.Value = dmx_ch[(((panelSelected - 1) * 24) + 10)];
            labValue10.Text = dmx_ch[(((panelSelected - 1) * 24) + 10)].ToString();
        }

        private void btnOff11_Click(object sender, EventArgs e)
        {
            dmx_write(11, 0);
            trackBar11.Value = dmx_ch[(((panelSelected - 1) * 24) + 11)];
            labValue11.Text = dmx_ch[(((panelSelected - 1) * 24) + 11)].ToString();
        }

        private void btnOff12_Click(object sender, EventArgs e)
        {
            dmx_write(12, 0);
            trackBar12.Value = dmx_ch[(((panelSelected - 1) * 24) + 12)];
            labValue12.Text = dmx_ch[(((panelSelected - 1) * 24) + 12)].ToString();
        }

        private void btnOff13_Click(object sender, EventArgs e)
        {
            dmx_write(13, 0);
            trackBar13.Value = dmx_ch[(((panelSelected - 1) * 24) + 13)];
            labValue13.Text = dmx_ch[(((panelSelected - 1) * 24) + 13)].ToString();
        }


        private void btnOff14_Click(object sender, EventArgs e)
        {
            dmx_write(14, 0);
            trackBar14.Value = dmx_ch[(((panelSelected - 1) * 24) + 14)];
            labValue14.Text = dmx_ch[(((panelSelected - 1) * 24) + 14)].ToString();
        }

        private void btnOff15_Click(object sender, EventArgs e)
        {
            dmx_write(15, 0);
            trackBar15.Value = dmx_ch[(((panelSelected - 1) * 24) + 15)];
            labValue15.Text = dmx_ch[(((panelSelected - 1) * 24) + 15)].ToString();
        }

        private void btnOff16_Click(object sender, EventArgs e)
        {
            dmx_write(16, 0);
            trackBar16.Value = dmx_ch[(((panelSelected - 1) * 24) + 16)];
            labValue16.Text = dmx_ch[(((panelSelected - 1) * 24) + 16)].ToString();
        }

        private void btnOff17_Click(object sender, EventArgs e)
        {
            dmx_write(17, 0);
            trackBar17.Value = dmx_ch[(((panelSelected - 1) * 24) + 17)];
            labValue17.Text = dmx_ch[(((panelSelected - 1) * 24) + 17)].ToString();
        }

        private void btnOff18_Click(object sender, EventArgs e)
        {
            dmx_write(18, 0);
            trackBar18.Value = dmx_ch[(((panelSelected - 1) * 24) + 18)];
            labValue18.Text = dmx_ch[(((panelSelected - 1) * 24) + 18)].ToString();
        }

        private void btnOff19_Click(object sender, EventArgs e)
        {
            dmx_write(19, 0);
            trackBar19.Value = dmx_ch[(((panelSelected - 1) * 24) + 19)];
            labValue19.Text = dmx_ch[(((panelSelected - 1) * 24) + 19)].ToString();
        }

        private void btnOff20_Click(object sender, EventArgs e)
        {
            dmx_write(20, 0);
            trackBar20.Value = dmx_ch[(((panelSelected - 1) * 24) + 20)];
            labValue20.Text = dmx_ch[(((panelSelected - 1) * 24) + 20)].ToString();
        }

        private void btnOff21_Click(object sender, EventArgs e)
        {
            dmx_write(21, 0);
            trackBar21.Value = dmx_ch[(((panelSelected - 1) * 24) + 21)];
            labValue21.Text = dmx_ch[(((panelSelected - 1) * 24) + 21)].ToString();
        }

        private void btnOff22_Click(object sender, EventArgs e)
        {
            dmx_write(22, 0);
            trackBar22.Value = dmx_ch[(((panelSelected - 1) * 24) + 22)];
            labValue22.Text = dmx_ch[(((panelSelected - 1) * 24) + 22)].ToString();
        }

        private void btnOff23_Click(object sender, EventArgs e)
        {
            dmx_write(23, 0);
            trackBar23.Value = dmx_ch[(((panelSelected - 1) * 24) + 23)];
            labValue23.Text = dmx_ch[(((panelSelected - 1) * 24) + 23)].ToString();
        }

        private void btnOff24_Click(object sender, EventArgs e)
        {
            dmx_write(24, 0);
            trackBar24.Value = dmx_ch[(((panelSelected - 1) * 24) + 24)];
            labValue24.Text = dmx_ch[(((panelSelected - 1) * 24) + 24)].ToString();
        }



        //____________________________________________________________________________________________________________


        private void btnOn1_25_Click(object sender, EventArgs e)
        {
            dmx_write(1, 64);
            trackBar1.Value = dmx_ch[(((panelSelected - 1) * 24) + 1)];
            labValue1.Text = dmx_ch[(((panelSelected - 1) * 24) + 1)].ToString();
        }

        private void btnOn2_25_Click(object sender, EventArgs e)
        {
            dmx_write(2, 64);
            trackBar2.Value = dmx_ch[(((panelSelected - 1) * 24) + 2)];
            labValue2.Text = dmx_ch[(((panelSelected - 1) * 24) + 2)].ToString();
        }

        private void btnOn3_25_Click(object sender, EventArgs e)
        {
            dmx_write(3, 64);
            trackBar3.Value = dmx_ch[(((panelSelected - 1) * 24) + 3)];
            labValue3.Text = dmx_ch[(((panelSelected - 1) * 24) + 3)].ToString();
        }

        private void btnOn4_25_Click(object sender, EventArgs e)
        {
            dmx_write(4, 64);
            trackBar4.Value = dmx_ch[(((panelSelected - 1) * 24) + 4)];
            labValue4.Text = dmx_ch[(((panelSelected - 1) * 24) + 4)].ToString();
        }

        private void btnOn5_25_Click(object sender, EventArgs e)
        {
            dmx_write(5, 64);
            trackBar5.Value = dmx_ch[(((panelSelected - 1) * 24) + 5)];
            labValue5.Text = dmx_ch[(((panelSelected - 1) * 24) + 5)].ToString();
        }

        private void btnOn6_25_Click(object sender, EventArgs e)
        {
            dmx_write(6, 64);
            trackBar6.Value = dmx_ch[(((panelSelected - 1) * 24) + 6)];
            labValue6.Text = dmx_ch[(((panelSelected - 1) * 24) + 6)].ToString();
        }

        private void btnOn7_25_Click(object sender, EventArgs e)
        {
            dmx_write(7, 64);
            trackBar7.Value = dmx_ch[(((panelSelected - 1) * 24) + 7)];
            labValue7.Text = dmx_ch[(((panelSelected - 1) * 24) + 7)].ToString();
        }

        private void btnOn8_25_Click(object sender, EventArgs e)
        {
            dmx_write(8, 64);
            trackBar8.Value = dmx_ch[(((panelSelected - 1) * 24) + 8)];
            labValue8.Text = dmx_ch[(((panelSelected - 1) * 24) + 8)].ToString();
        }

        private void btnOn9_25_Click(object sender, EventArgs e)
        {
            dmx_write(9, 64);
            trackBar9.Value = dmx_ch[(((panelSelected - 1) * 24) + 9)];
            labValue9.Text = dmx_ch[(((panelSelected - 1) * 24) + 9)].ToString();
        }

        private void btnOn10_25_Click(object sender, EventArgs e)
        {
            dmx_write(10, 64);
            trackBar10.Value = dmx_ch[(((panelSelected - 1) * 24) + 10)];
            labValue10.Text = dmx_ch[(((panelSelected - 1) * 24) + 10)].ToString();
        }

        private void btnOn11_25_Click(object sender, EventArgs e)
        {
            dmx_write(11, 64);
            trackBar11.Value = dmx_ch[(((panelSelected - 1) * 24) + 11)];
            labValue11.Text = dmx_ch[(((panelSelected - 1) * 24) + 11)].ToString();
        }

        private void btnOn12_25_Click(object sender, EventArgs e)
        {
            dmx_write(12, 64);
            trackBar12.Value = dmx_ch[(((panelSelected - 1) * 24) + 12)];
            labValue12.Text = dmx_ch[(((panelSelected - 1) * 24) + 12)].ToString();
        }

        private void btnOn13_25_Click(object sender, EventArgs e)
        {
            dmx_write(13, 64);
            trackBar13.Value = dmx_ch[(((panelSelected - 1) * 24) + 13)];
            labValue13.Text = dmx_ch[(((panelSelected - 1) * 24) + 13)].ToString();
        }

        private void btnOn14_25_Click(object sender, EventArgs e)
        {
            dmx_write(14, 64);
            trackBar14.Value = dmx_ch[(((panelSelected - 1) * 24) + 14)];
            labValue14.Text = dmx_ch[(((panelSelected - 1) * 24) + 14)].ToString();
        }

        private void btnOn15_25_Click(object sender, EventArgs e)
        {
            dmx_write(15, 64);
            trackBar15.Value = dmx_ch[(((panelSelected - 1) * 24) + 15)];
            labValue15.Text = dmx_ch[(((panelSelected - 1) * 24) + 15)].ToString();
        }

        private void btnOn16_25_Click(object sender, EventArgs e)
        {
            dmx_write(16, 64);
            trackBar16.Value = dmx_ch[(((panelSelected - 1) * 24) + 16)];
            labValue16.Text = dmx_ch[(((panelSelected - 1) * 24) + 16)].ToString();
        }

        private void btnOn17_25_Click(object sender, EventArgs e)
        {
            dmx_write(17, 64);
            trackBar17.Value = dmx_ch[(((panelSelected - 1) * 24) + 17)];
            labValue17.Text = dmx_ch[(((panelSelected - 1) * 24) + 17)].ToString();
        }

        private void btnOn18_25_Click(object sender, EventArgs e)
        {
            dmx_write(18, 64);
            trackBar18.Value = dmx_ch[(((panelSelected - 1) * 24) + 18)];
            labValue18.Text = dmx_ch[(((panelSelected - 1) * 24) + 18)].ToString();
        }

        private void btnOn19_25_Click(object sender, EventArgs e)
        {
            dmx_write(19, 64);
            trackBar19.Value = dmx_ch[(((panelSelected - 1) * 24) + 19)];
            labValue19.Text = dmx_ch[(((panelSelected - 1) * 24) + 19)].ToString();
        }

        private void btnOn20_25_Click(object sender, EventArgs e)
        {
            dmx_write(20, 64);
            trackBar20.Value = dmx_ch[(((panelSelected - 1) * 24) + 20)];
            labValue20.Text = dmx_ch[(((panelSelected - 1) * 24) + 20)].ToString();
        }

        private void btnOn21_25_Click(object sender, EventArgs e)
        {
            dmx_write(21, 64);
            trackBar21.Value = dmx_ch[(((panelSelected - 1) * 24) + 21)];
            labValue21.Text = dmx_ch[(((panelSelected - 1) * 24) + 21)].ToString();
        }

        private void btnOn22_25_Click(object sender, EventArgs e)
        {
            dmx_write(22, 64);
            trackBar22.Value = dmx_ch[(((panelSelected - 1) * 24) + 22)];
            labValue22.Text = dmx_ch[(((panelSelected - 1) * 24) + 22)].ToString();
        }

        private void btnOn23_25_Click(object sender, EventArgs e)
        {
            dmx_write(23, 64);
            trackBar23.Value = dmx_ch[(((panelSelected - 1) * 24) + 23)];
            labValue23.Text = dmx_ch[(((panelSelected - 1) * 24) + 23)].ToString();
        }

        private void btnOn24_25_Click(object sender, EventArgs e)
        {
            dmx_write(24, 64);
            trackBar24.Value = dmx_ch[(((panelSelected - 1) * 24) + 24)];
            labValue24.Text = dmx_ch[(((panelSelected - 1) * 24) + 24)].ToString();
        }


        //____________________________________________________________________________________________________________


        private void btnOn1_50_Click(object sender, EventArgs e)
        {
            dmx_write(1, 128);
            trackBar1.Value = dmx_ch[(((panelSelected - 1) * 24) + 1)];
            labValue1.Text = dmx_ch[(((panelSelected - 1) * 24) + 1)].ToString();
        }

        private void btnOn2_50_Click(object sender, EventArgs e)
        {
            dmx_write(2, 128);
            trackBar2.Value = dmx_ch[(((panelSelected - 1) * 24) + 2)];
            labValue2.Text = dmx_ch[(((panelSelected - 1) * 24) + 2)].ToString();
        }

        private void btnOn3_50_Click(object sender, EventArgs e)
        {
            dmx_write(3, 128);
            trackBar3.Value = dmx_ch[(((panelSelected - 1) * 24) + 3)];
            labValue3.Text = dmx_ch[(((panelSelected - 1) * 24) + 3)].ToString();
        }

        private void btnOn4_50_Click(object sender, EventArgs e)
        {
            dmx_write(4, 128);
            trackBar4.Value = dmx_ch[(((panelSelected - 1) * 24) + 4)];
            labValue4.Text = dmx_ch[(((panelSelected - 1) * 24) + 4)].ToString();
        }

        private void btnOn5_50_Click(object sender, EventArgs e)
        {
            dmx_write(5, 128);
            trackBar5.Value = dmx_ch[(((panelSelected - 1) * 24) + 5)];
            labValue5.Text = dmx_ch[(((panelSelected - 1) * 24) + 5)].ToString();
        }

        private void btnOn6_50_Click(object sender, EventArgs e)
        {
            dmx_write(6, 128);
            trackBar6.Value = dmx_ch[(((panelSelected - 1) * 24) + 6)];
            labValue6.Text = dmx_ch[(((panelSelected - 1) * 24) + 6)].ToString();
        }

        private void btnOn7_50_Click(object sender, EventArgs e)
        {
            dmx_write(7, 128);
            trackBar7.Value = dmx_ch[(((panelSelected - 1) * 24) + 7)];
            labValue7.Text = dmx_ch[(((panelSelected - 1) * 24) + 7)].ToString();
        }

        private void btnOn8_50_Click(object sender, EventArgs e)
        {
            dmx_write(8, 128);
            trackBar8.Value = dmx_ch[(((panelSelected - 1) * 24) + 8)];
            labValue8.Text = dmx_ch[(((panelSelected - 1) * 24) + 8)].ToString();
        }

        private void btnOn9_50_Click(object sender, EventArgs e)
        {
            dmx_write(9, 128);
            trackBar9.Value = dmx_ch[(((panelSelected - 1) * 24) + 9)];
            labValue9.Text = dmx_ch[(((panelSelected - 1) * 24) + 9)].ToString();
        }

        private void btnOn10_50_Click(object sender, EventArgs e)
        {
            dmx_write(10, 128);
            trackBar10.Value = dmx_ch[(((panelSelected - 1) * 24) + 10)];
            labValue10.Text = dmx_ch[(((panelSelected - 1) * 24) + 10)].ToString();
        }

        private void btnOn11_50_Click(object sender, EventArgs e)
        {
            dmx_write(11, 128);
            trackBar11.Value = dmx_ch[(((panelSelected - 1) * 24) + 11)];
            labValue11.Text = dmx_ch[(((panelSelected - 1) * 24) + 11)].ToString();
        }

        private void btnOn12_50_Click(object sender, EventArgs e)
        {
            dmx_write(12, 128);
            trackBar12.Value = dmx_ch[(((panelSelected - 1) * 24) + 12)];
            labValue12.Text = dmx_ch[(((panelSelected - 1) * 24) + 12)].ToString();
        }

        private void btnOn13_50_Click(object sender, EventArgs e)
        {
            dmx_write(13, 128);
            trackBar13.Value = dmx_ch[(((panelSelected - 1) * 24) + 13)];
            labValue13.Text = dmx_ch[(((panelSelected - 1) * 24) + 13)].ToString();
        }

        private void btnOn14_50_Click(object sender, EventArgs e)
        {
            dmx_write(14, 128);
            trackBar14.Value = dmx_ch[(((panelSelected - 1) * 24) + 14)];
            labValue14.Text = dmx_ch[(((panelSelected - 1) * 24) + 14)].ToString();
        }

        private void btnOn15_50_Click(object sender, EventArgs e)
        {
            dmx_write(15, 128);
            trackBar15.Value = dmx_ch[(((panelSelected - 1) * 24) + 15)];
            labValue15.Text = dmx_ch[(((panelSelected - 1) * 24) + 15)].ToString();
        }

        private void btnOn16_50_Click(object sender, EventArgs e)
        {
            dmx_write(16, 128);
            trackBar16.Value = dmx_ch[(((panelSelected - 1) * 24) + 16)];
            labValue16.Text = dmx_ch[(((panelSelected - 1) * 24) + 16)].ToString();
        }

        private void btnOn17_50_Click(object sender, EventArgs e)
        {
            dmx_write(17, 128);
            trackBar17.Value = dmx_ch[(((panelSelected - 1) * 24) + 17)];
            labValue17.Text = dmx_ch[(((panelSelected - 1) * 24) + 17)].ToString();
        }

        private void btnOn18_50_Click(object sender, EventArgs e)
        {
            dmx_write(18, 128);
            trackBar18.Value = dmx_ch[(((panelSelected - 1) * 24) + 18)];
            labValue18.Text = dmx_ch[(((panelSelected - 1) * 24) + 18)].ToString();
        }

        private void btnOn19_50_Click(object sender, EventArgs e)
        {
            dmx_write(19, 128);
            trackBar19.Value = dmx_ch[(((panelSelected - 1) * 24) + 19)];
            labValue19.Text = dmx_ch[(((panelSelected - 1) * 24) + 19)].ToString();
        }

        private void btnOn20_50_Click(object sender, EventArgs e)
        {
            dmx_write(20, 128);
            trackBar20.Value = dmx_ch[(((panelSelected - 1) * 24) + 20)];
            labValue20.Text = dmx_ch[(((panelSelected - 1) * 24) + 20)].ToString();
        }

        private void btnOn21_50_Click(object sender, EventArgs e)
        {
            dmx_write(21, 128);
            trackBar21.Value = dmx_ch[(((panelSelected - 1) * 24) + 21)];
            labValue21.Text = dmx_ch[(((panelSelected - 1) * 24) + 21)].ToString();
        }

        private void btnOn22_50_Click(object sender, EventArgs e)
        {
            dmx_write(22, 128);
            trackBar22.Value = dmx_ch[(((panelSelected - 1) * 24) + 22)];
            labValue22.Text = dmx_ch[(((panelSelected - 1) * 24) + 22)].ToString();
        }

        private void btnOn23_50_Click(object sender, EventArgs e)
        {
            dmx_write(23, 128);
            trackBar23.Value = dmx_ch[(((panelSelected - 1) * 24) + 23)];
            labValue23.Text = dmx_ch[(((panelSelected - 1) * 24) + 23)].ToString();
        }

        private void btnOn24_50_Click(object sender, EventArgs e)
        {
            dmx_write(24, 128);
            trackBar24.Value = dmx_ch[(((panelSelected - 1) * 24) + 24)];
            labValue24.Text = dmx_ch[(((panelSelected - 1) * 24) + 24)].ToString();
        }


        //____________________________________________________________________________________________________________


        private void btnOn1_75_Click(object sender, EventArgs e)
        {
            dmx_write(1, 192);
            trackBar1.Value = dmx_ch[(((panelSelected - 1) * 24) + 1)];
            labValue1.Text = dmx_ch[(((panelSelected - 1) * 24) + 1)].ToString();
        }

        private void btnOn2_75_Click(object sender, EventArgs e)
        {
            dmx_write(2, 192);
            trackBar2.Value = dmx_ch[(((panelSelected - 1) * 24) + 2)];
            labValue2.Text = dmx_ch[(((panelSelected - 1) * 24) + 2)].ToString();
        }

        private void btnOn3_75_Click(object sender, EventArgs e)
        {
            dmx_write(3, 192);
            trackBar3.Value = dmx_ch[(((panelSelected - 1) * 24) + 3)];
            labValue3.Text = dmx_ch[(((panelSelected - 1) * 24) + 3)].ToString();
        }

        private void btnOn4_75_Click(object sender, EventArgs e)
        {
            dmx_write(4, 192);
            trackBar4.Value = dmx_ch[(((panelSelected - 1) * 24) + 4)];
            labValue4.Text = dmx_ch[(((panelSelected - 1) * 24) + 4)].ToString();
        }

        private void btnOn5_75_Click(object sender, EventArgs e)
        {
            dmx_write(5, 192);
            trackBar5.Value = dmx_ch[(((panelSelected - 1) * 24) + 5)];
            labValue5.Text = dmx_ch[(((panelSelected - 1) * 24) + 5)].ToString();
        }

        private void btnOn6_75_Click(object sender, EventArgs e)
        {
            dmx_write(6, 192);
            trackBar6.Value = dmx_ch[(((panelSelected - 1) * 24) + 6)];
            labValue6.Text = dmx_ch[(((panelSelected - 1) * 24) + 6)].ToString();
        }

        private void btnOn7_75_Click(object sender, EventArgs e)
        {
            dmx_write(7, 192);
            trackBar7.Value = dmx_ch[(((panelSelected - 1) * 24) + 7)];
            labValue7.Text = dmx_ch[(((panelSelected - 1) * 24) + 7)].ToString();
        }

        private void btnOn8_75_Click(object sender, EventArgs e)
        {
            dmx_write(8, 192);
            trackBar8.Value = dmx_ch[(((panelSelected - 1) * 24) + 8)];
            labValue8.Text = dmx_ch[(((panelSelected - 1) * 24) + 8)].ToString();
        }

        private void btnOn9_75_Click(object sender, EventArgs e)
        {
            dmx_write(9, 192);
            trackBar9.Value = dmx_ch[(((panelSelected - 1) * 24) + 9)];
            labValue9.Text = dmx_ch[(((panelSelected - 1) * 24) + 9)].ToString();
        }

        private void btnOn10_75_Click(object sender, EventArgs e)
        {
            dmx_write(10, 192);
            trackBar10.Value = dmx_ch[(((panelSelected - 1) * 24) + 10)];
            labValue10.Text = dmx_ch[(((panelSelected - 1) * 24) + 10)].ToString();
        }

        private void btnOn11_75_Click(object sender, EventArgs e)
        {
            dmx_write(11, 192);
            trackBar11.Value = dmx_ch[(((panelSelected - 1) * 24) + 11)];
            labValue11.Text = dmx_ch[(((panelSelected - 1) * 24) + 11)].ToString();
        }

        private void btnOn12_75_Click(object sender, EventArgs e)
        {
            dmx_write(12, 192);
            trackBar12.Value = dmx_ch[(((panelSelected - 1) * 24) + 12)];
            labValue12.Text = dmx_ch[(((panelSelected - 1) * 24) + 12)].ToString();
        }

        private void btnOn13_75_Click(object sender, EventArgs e)
        {
            dmx_write(13, 192);
            trackBar13.Value = dmx_ch[(((panelSelected - 1) * 24) + 13)];
            labValue13.Text = dmx_ch[(((panelSelected - 1) * 24) + 13)].ToString();
        }

        private void btnOn14_75_Click_1(object sender, EventArgs e)
        {
            dmx_write(14, 192);
            trackBar14.Value = dmx_ch[(((panelSelected - 1) * 24) + 14)];
            labValue14.Text = dmx_ch[(((panelSelected - 1) * 24) + 14)].ToString();

        }

        private void btnOn15_75_Click(object sender, EventArgs e)
        {
            dmx_write(15, 192);
            trackBar15.Value = dmx_ch[(((panelSelected - 1) * 24) + 15)];
            labValue15.Text = dmx_ch[(((panelSelected - 1) * 24) + 15)].ToString();
        }

        private void btnOn16_75_Click(object sender, EventArgs e)
        {
            dmx_write(16, 192);
            trackBar16.Value = dmx_ch[(((panelSelected - 1) * 24) + 16)];
            labValue16.Text = dmx_ch[(((panelSelected - 1) * 24) + 16)].ToString();
        }

        private void btnOn17_75_Click(object sender, EventArgs e)
        {
            dmx_write(17, 192);
            trackBar17.Value = dmx_ch[(((panelSelected - 1) * 24) + 17)];
            labValue17.Text = dmx_ch[(((panelSelected - 1) * 24) + 17)].ToString();
        }

        private void btnOn18_75_Click(object sender, EventArgs e)
        {
            dmx_write(18, 192);
            trackBar18.Value = dmx_ch[(((panelSelected - 1) * 24) + 18)];
            labValue18.Text = dmx_ch[(((panelSelected - 1) * 24) + 18)].ToString();
        }

        private void btnOn19_75_Click(object sender, EventArgs e)
        {
            dmx_write(19, 192);
            trackBar19.Value = dmx_ch[(((panelSelected - 1) * 24) + 19)];
            labValue19.Text = dmx_ch[(((panelSelected - 1) * 24) + 19)].ToString();
        }

        private void btnOn20_75_Click(object sender, EventArgs e)
        {
            dmx_write(20, 192);
            trackBar20.Value = dmx_ch[(((panelSelected - 1) * 24) + 20)];
            labValue20.Text = dmx_ch[(((panelSelected - 1) * 24) + 20)].ToString();
        }

        private void btnOn21_75_Click(object sender, EventArgs e)
        {
            dmx_write(21, 192);
            trackBar21.Value = dmx_ch[(((panelSelected - 1) * 24) + 21)];
            labValue21.Text = dmx_ch[(((panelSelected - 1) * 24) + 21)].ToString();
        }

        private void btnOn22_75_Click(object sender, EventArgs e)
        {
            dmx_write(22, 192);
            trackBar22.Value = dmx_ch[(((panelSelected - 1) * 24) + 22)];
            labValue22.Text = dmx_ch[(((panelSelected - 1) * 24) + 22)].ToString();
        }

        private void btnOn23_75_Click(object sender, EventArgs e)
        {
            dmx_write(23, 192);
            trackBar23.Value = dmx_ch[(((panelSelected - 1) * 24) + 23)];
            labValue23.Text = dmx_ch[(((panelSelected - 1) * 24) + 23)].ToString();
        }

        private void btnOn24_75_Click(object sender, EventArgs e)
        {
            dmx_write(24, 192);
            trackBar24.Value = dmx_ch[(((panelSelected - 1) * 24) + 24)];
            labValue24.Text = dmx_ch[(((panelSelected - 1) * 24) + 24)].ToString();
        }



        //____________________________________________________________________________________________________________


        private void trackBar1_Scroll_1(object sender, EventArgs e)
        {
            dmx_write(1, trackBar1.Value);
            trackBar1.Value = dmx_ch[(((panelSelected - 1) * 24) + 1)];
            labValue1.Text = dmx_ch[(((panelSelected - 1) * 24) + 1)].ToString();
        }

        private void trackBar2_Scroll_1(object sender, EventArgs e)
        {
            dmx_write(2, trackBar2.Value);
            trackBar2.Value = dmx_ch[(((panelSelected - 1) * 24) + 2)];
            labValue2.Text = dmx_ch[(((panelSelected - 1) * 24) + 2)].ToString();
        }

        private void trackBar3_Scroll_1(object sender, EventArgs e)
        {
            dmx_write(3, trackBar3.Value);
            trackBar3.Value = dmx_ch[(((panelSelected - 1) * 24) + 3)];
            labValue3.Text = dmx_ch[(((panelSelected - 1) * 24) + 3)].ToString();
        }

        private void trackBar4_Scroll(object sender, EventArgs e)
        {
            dmx_write(4, trackBar4.Value);
            trackBar4.Value = dmx_ch[(((panelSelected - 1) * 24) + 4)];
            labValue4.Text = dmx_ch[(((panelSelected - 1) * 24) + 4)].ToString();
        }

        private void trackBar5_Scroll(object sender, EventArgs e)
        {
            dmx_write(5, trackBar5.Value);
            trackBar5.Value = dmx_ch[(((panelSelected - 1) * 24) + 5)];
            labValue5.Text = dmx_ch[(((panelSelected - 1) * 24) + 5)].ToString();
        }

        private void trackBar6_Scroll(object sender, EventArgs e)
        {
            dmx_write(6, trackBar6.Value);
            trackBar6.Value = dmx_ch[(((panelSelected - 1) * 24) + 6)];
            labValue6.Text = dmx_ch[(((panelSelected - 1) * 24) + 6)].ToString();
        }

        private void trackBar7_Scroll(object sender, EventArgs e)
        {
            dmx_write(7, trackBar7.Value);
            trackBar7.Value = dmx_ch[(((panelSelected - 1) * 24) + 7)];
            labValue7.Text = dmx_ch[(((panelSelected - 1) * 24) + 7)].ToString();
        }

        private void trackBar8_Scroll(object sender, EventArgs e)
        {
            dmx_write(8, trackBar8.Value);
            trackBar8.Value = dmx_ch[(((panelSelected - 1) * 24) + 8)];
            labValue8.Text = dmx_ch[(((panelSelected - 1) * 24) + 8)].ToString();
        }

        private void trackBar9_Scroll(object sender, EventArgs e)
        {
            dmx_write(9, trackBar9.Value);
            trackBar9.Value = dmx_ch[(((panelSelected - 1) * 24) + 9)];
            labValue9.Text = dmx_ch[(((panelSelected - 1) * 24) + 9)].ToString();
        }

        private void trackBar10_Scroll(object sender, EventArgs e)
        {
            dmx_write(10, trackBar10.Value);
            trackBar10.Value = dmx_ch[(((panelSelected - 1) * 24) + 10)];
            labValue10.Text = dmx_ch[(((panelSelected - 1) * 24) + 10)].ToString();
        }

        private void trackBar11_Scroll(object sender, EventArgs e)
        {
            dmx_write(11, trackBar11.Value);
            trackBar11.Value = dmx_ch[(((panelSelected - 1) * 24) + 11)];
            labValue11.Text = dmx_ch[(((panelSelected - 1) * 24) + 11)].ToString();
        }

        private void trackBar12_Scroll(object sender, EventArgs e)
        {
            dmx_write(12, trackBar12.Value);
            trackBar12.Value = dmx_ch[(((panelSelected - 1) * 24) + 12)];
            labValue12.Text = dmx_ch[(((panelSelected - 1) * 24) + 12)].ToString();
        }

        private void trackBar13_Scroll(object sender, EventArgs e)
        {
            dmx_write(13, trackBar13.Value);
            trackBar13.Value = dmx_ch[(((panelSelected - 1) * 24) + 13)];
            labValue13.Text = dmx_ch[(((panelSelected - 1) * 24) + 13)].ToString();
        }

        private void trackBar14_Scroll(object sender, EventArgs e)
        {
            dmx_write(14, trackBar14.Value);
            trackBar14.Value = dmx_ch[(((panelSelected - 1) * 24) + 14)];
            labValue14.Text = dmx_ch[(((panelSelected - 1) * 24) + 14)].ToString();
        }

        private void trackBar15_Scroll(object sender, EventArgs e)
        {
            dmx_write(15, trackBar15.Value);
            trackBar15.Value = dmx_ch[(((panelSelected - 1) * 24) + 15)];
            labValue15.Text = dmx_ch[(((panelSelected - 1) * 24) + 15)].ToString();
        }

        private void trackBar16_Scroll(object sender, EventArgs e)
        {
            dmx_write(16, trackBar16.Value);
            trackBar16.Value = dmx_ch[(((panelSelected - 1) * 24) + 16)];
            labValue16.Text = dmx_ch[(((panelSelected - 1) * 24) + 16)].ToString();
        }

        private void trackBar17_Scroll(object sender, EventArgs e)
        {
            dmx_write(17, trackBar17.Value);
            trackBar17.Value = dmx_ch[(((panelSelected - 1) * 24) + 17)];
            labValue17.Text = dmx_ch[(((panelSelected - 1) * 24) + 17)].ToString();
        }

        private void trackBar18_Scroll(object sender, EventArgs e)
        {
            dmx_write(18, trackBar18.Value);
            trackBar18.Value = dmx_ch[(((panelSelected - 1) * 24) + 18)];
            labValue18.Text = dmx_ch[(((panelSelected - 1) * 24) + 18)].ToString();
        }

        private void trackBar19_Scroll(object sender, EventArgs e)
        {
            dmx_write(19, trackBar19.Value);
            trackBar19.Value = dmx_ch[(((panelSelected - 1) * 24) + 19)];
            labValue19.Text = dmx_ch[(((panelSelected - 1) * 24) + 19)].ToString();
        }

        private void trackBar20_Scroll(object sender, EventArgs e)
        {
            dmx_write(20, trackBar20.Value);
            trackBar20.Value = dmx_ch[(((panelSelected - 1) * 24) + 20)];
            labValue20.Text = dmx_ch[(((panelSelected - 1) * 24) + 20)].ToString();
        }

        private void trackBar21_Scroll(object sender, EventArgs e)
        {
            dmx_write(21, trackBar21.Value);
            trackBar21.Value = dmx_ch[(((panelSelected - 1) * 24) + 21)];
            labValue21.Text = dmx_ch[(((panelSelected - 1) * 24) + 21)].ToString();
        }

        private void trackBar22_Scroll(object sender, EventArgs e)
        {
            dmx_write(22, trackBar22.Value);
            trackBar22.Value = dmx_ch[(((panelSelected - 1) * 24) + 22)];
            labValue22.Text = dmx_ch[(((panelSelected - 1) * 24) + 22)].ToString();
        }

        private void trackBar23_Scroll(object sender, EventArgs e)
        {
            dmx_write(23, trackBar23.Value);
            trackBar23.Value = dmx_ch[(((panelSelected - 1) * 24) + 23)];
            labValue23.Text = dmx_ch[(((panelSelected - 1) * 24) + 23)].ToString();
        }

        private void trackBar24_Scroll(object sender, EventArgs e)
        {
            dmx_write(24, trackBar24.Value);
            trackBar24.Value = dmx_ch[(((panelSelected - 1) * 24) + 24)];
            labValue24.Text = dmx_ch[(((panelSelected - 1) * 24) + 24)].ToString();
        }
        
        
        //____________________________________________________________________________________________________________

        private void btnSequencerToggle_Click(object sender, EventArgs e)
        {
            if(seqEnable == false)
            {
                seq = 1;
                seqTimer.Interval = Convert.ToInt16(Convert.ToSingle(sequencerTimeSelect.Value) * 1000);
                seqEnable = true;
                seqTimer.Enabled = true;
                btnSequencerToggle.Text = "Sequencer OFF";
                performSeq();

            }
            else if (seqEnable == true)
            {
                seqEnable = false;
                seqTimer.Enabled = false;
                btnSequencerToggle.Text = "Sequencer ON";
                SeqS1.ForeColor = Color.Black;
                SeqS2.ForeColor = Color.Black;
                SeqS3.ForeColor = Color.Black;
                SeqS4.ForeColor = Color.Black;
                SeqS5.ForeColor = Color.Black;
                SeqS6.ForeColor = Color.Black;
            }
        }

        private void sequencerTimeSelect_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                seqTimer.Interval = Convert.ToInt16(Convert.ToSingle(sequencerTimeSelect.Value)*1000);
                
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
            
        }

        private void seqTimer_Tick(object sender, EventArgs e)
        {
            performSeq();
        }


        void performSeq()
        {
            SeqS1.ForeColor = Color.Black;
            SeqS2.ForeColor = Color.Black;
            SeqS3.ForeColor = Color.Black;
            SeqS4.ForeColor = Color.Black;
            SeqS5.ForeColor = Color.Black;
            SeqS6.ForeColor = Color.Black;

            if (SeqS1.Checked == true)
            {
                numSeq[0]=1;
            }
            else
            {
                numSeq[0]=0;    
            }

            if (SeqS2.Checked == true)
            {
                numSeq[1] = 1;
            }
            else
            {
                numSeq[1] = 0;
            }

            if (SeqS3.Checked == true)
            {
                numSeq[2] = 1;
            }
            else
            {
                numSeq[2] = 0;
            }

            if (SeqS4.Checked == true)
            {
                numSeq[3] = 1;
            }
            else
            {
                numSeq[3] = 0;
            }

            if (SeqS5.Checked == true)
            {
                numSeq[4] = 1;
            }
            else
            {
                numSeq[4] = 0;
            }

            if (SeqS6.Checked == true)
            {
                numSeq[5] = 1;
            }
            else
            {
                numSeq[5] = 0;
            }


            
            Start:

            while (numSeq[seq-1] != 1)
            {   
                seq++;   
                
                if (seq >= 6)
                    break;
                
            }

            int remain = 0;
            for(int i=seq-1; i<numSeq.Length; i++)
            {
                remain += numSeq[i];
            }

            if(remain == 0)
            {
                seq = 1;
                goto Start; 
            }


            switch (seq)
            {
                case 1:
                    btnS1.PerformClick();
                    SeqS1.ForeColor = Color.ForestGreen;
                    break;
                case 2:
                    btnS2.PerformClick();
                    SeqS2.ForeColor = Color.ForestGreen;
                    break;
                case 3:
                    btnS3.PerformClick();
                    SeqS3.ForeColor = Color.ForestGreen;
                    break;
                case 4:
                    btnS4.PerformClick();
                    SeqS4.ForeColor = Color.ForestGreen;
                    break;
                case 5:
                    btnS5.PerformClick();
                    SeqS5.ForeColor = Color.ForestGreen;
                    break;
                case 6:
                    btnS6.PerformClick();
                    SeqS6.ForeColor = Color.ForestGreen;
                    break;
                default: break;
            }

            seq++;

            if (seq > 6)
            {
                seq = 1;
            }


            }


    }
}
