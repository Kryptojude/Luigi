using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Luigi
{
    public partial class Form1 : Form
    {
        /* IDEEN:
        * zelda overworld
        * NPC
        * Items einfügen (DreckWeg/SchreckWeg)
        * Teamarbeit (Knopf öffnet Tür, wiederbeleben)
        * Laterne
        * Geist aus Boden GIF (Friedhof)
        * Lagerfeuer GIF + Toad :)
        * Mario Odyssey Mütze Geister kontrollieren
        * Lebensleiste
        */

        //DeveloperMode Sachendinge
        bool developerMode = false;
        bool bKeyPressed = false;
        int textureIndex = 0;
        Bitmap[] textureArray;

        //Spielsachendinge
        PictureBox canvas;
        Panel collisionBoxesContainer;
        Bitmap mapMaker = new Bitmap(600, 750);
        PictureBox[,] collisionBoxes;
        bool gameOver = false;

        public class Player
        {
            public int x; //X-Koordinate des Spielers
            public int y; //Y-Koordinate des Spielers
            public bool canMove; //Ob sich der Spieler bewegen kann
            public Image icon;
            public Player(int px, int py, bool pnom, Image picon)
            {
                x = px;
                y = py;
                canMove = pnom;
                icon = picon;
            }
        }
        Player luigi = new Player(30 * 12, 30 * 23, true, Image.FromFile("images/luigi.png"));
        Player mario = new Player(30 *  1, 30 * 23, true, Image.FromFile("images/mario.png"));
        //Image lantern = Image.FromFile("images/lantern.png");

        public class Ghost
        {
            public int x; //X-Koordinate des Geistes
            public int y; //Y-Koordinate des Geistes
            public int mv; //Um wie viel bewegt sich der Geist?
            public Image icon;
            public Ghost(int gx, int gy, int gmv, Image picon)
            {
                x = gx;
                y = gy;
                mv = gmv;
                icon = picon;
            }
        }
        Ghost boo = new Ghost( 18 * 30, 13 * 30, -3, Image.FromFile("images/boo.png"));
        Ghost ghost1 = new Ghost(1 * 30,  3 * 30,  1, Image.FromFile("images/ghost1.png"));

        public class Area
        {
            public string name;
            public Bitmap background;
            public Area(string pname, Bitmap pbackground)
            {
                name = pname;
                background = pbackground;
            }
        }
        Area area = new Area("start", (Bitmap)Bitmap.FromFile("images/labyrinth.png"));

        public Form1()
        {
            InitializeComponent();

            this.Size = new Size(20 * 30 + 16, 25 * 30 + 39); //600 + Rand * 750 + Rand
            this.KeyDown += MovePlayers; //Bewegen Luigi und Mario
            this.KeyUp += CanMoveAgain; //Gedrückt halten verhindern

            canvas = new PictureBox //Spielfeld
            {
                Dock = DockStyle.Fill,
            };
            this.Controls.Add(canvas);
            canvas.Paint += PaintCanvas;

            collisionBoxesContainer = new Panel { Dock = DockStyle.Fill, };
            this.Controls.Add(collisionBoxesContainer);

            collisionBoxes = new PictureBox[25, 20];
            for (int y = 0; y < 25; y++) //collisionBoxes erstellen
            {
                for (int x = 0; x < 20; x++)
                {
                    collisionBoxes[y, x] = new PictureBox();
                    collisionBoxesContainer.Controls.Add(collisionBoxes[y, x]);
                    collisionBoxes[y, x].Size = new Size(30, 30);
                    collisionBoxes[y, x].Left = x * 30;
                    collisionBoxes[y, x].Top = y * 30;
                    collisionBoxes[y, x].BorderStyle = BorderStyle.FixedSingle;
                    collisionBoxes[y, x].MouseClick += ShowCoordinates; //In eine Methode?
                    collisionBoxes[y, x].MouseMove += LevelMaker;
                    collisionBoxes[y, x].MouseClick += LevelMaker;
                }
            }

            LoadArea(); //Anfangsbereich laden
            
            Timer motor = new Timer
            {
                Interval = 10,
            };
            motor.Tick += Motor;
            motor.Start();

            Bitmap leaves = new Bitmap("images/textures/newLeaves.png");
            Bitmap podzol = new Bitmap("images/textures/podzol.png");
            textureArray = new Bitmap[] { leaves, podzol };

            // B-Taste + Mausklick = collisionBoxes Koordinaten anzeigen
            this.KeyDown += delegate (object sender, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.B) bKeyPressed = true;
                if (e.KeyCode == Keys.T) save.Save("blub.png");
                if (e.KeyCode == Keys.L)
                {
                    textureIndex++;
                    if (textureIndex > textureArray.Length - 1)
                    {
                        textureIndex = 0;
                    }
                }
            };
            this.KeyUp += delegate (object sender, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.B) bKeyPressed = false;
            };
            canvas.MouseClick += ShowCoordinates;
        }

        private void PaintCanvas(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(boo.icon, boo.x, boo.y, 30, 30);
            e.Graphics.DrawImage(ghost1.icon, ghost1.x, ghost1.y, 30, 30);
            e.Graphics.DrawImage(luigi.icon, luigi.x, luigi.y, 30, 30);
            e.Graphics.DrawImage(mario.icon, mario.x, mario.y, 30, 30);
            if(gameOver) e.Graphics.DrawImage(new Bitmap("images/over.png"), 0, 0, 300, 300);
            //e.Graphics.DrawImage(lantern, mario.x -785, mario.y -985); //Laterne
        }

        private void MovePlayers(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return) //Entwickler-Modus ein-/ausschalten
            {
                DeveloperMode();
            }
            
            if (luigi.canMove) //Bewegt Luigi
            {
                switch (e.KeyCode)
                {
                    case Keys.Up: luigi.y -= 30; luigi.canMove = false;
                        break;
                    case Keys.Left: luigi.x -= 30; luigi.canMove = false;
                        break;
                    case Keys.Right: luigi.x += 30; luigi.canMove = false;
                        break;
                    case Keys.Down: luigi.y += 30; luigi.canMove = false;
                        break;

                }
            }
            if (mario.canMove) //Bewegt Mario
            {
                switch (e.KeyCode)
                {
                    case Keys.W: mario.y -= 30; mario.canMove = false;
                        break;
                    case Keys.A: mario.x -= 30; mario.canMove = false;
                        break;
                    case Keys.S: mario.y += 30; mario.canMove = false;
                        break;
                    case Keys.D: mario.x += 30; mario.canMove = false;
                        break;
                }
            }

            if (mario.x == luigi.x && mario.y == luigi.y) //Spieler Kollision
            {
                ReversePlayerMove(e.KeyCode);
            }

            if (collisionBoxes[luigi.y / 30, luigi.x / 30].Name == "unpassable" ||
                collisionBoxes[mario.y / 30, mario.x / 30].Name == "unpassable") //Spieler Wand Kollision
            {
                collisionBoxes[luigi.y / 30, luigi.x / 30].BackColor = Color.Red; //Debug-Farbe
                ReversePlayerMove(e.KeyCode);
            }

            //if ((mario.x == 11*30 && mario.y == 17*30) || (luigi.x == 11*30 && luigi.y == 17*30)) //Spieler Stufen Kollision
            //{
            //    area.background.Dispose();
            //    area.background = (Bitmap)Bitmap.FromFile("images/underground.png");
            //    LoadArea(); //Neuen Bereich laden
            //}
        }

        private void CanMoveAgain(object sender, KeyEventArgs e) //Bewegen ist deaktiviert, bis Taste losgelassen wird
        {
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down || e.KeyCode == Keys.Left || e.KeyCode == Keys.Right) luigi.canMove = true;
            if (e.KeyCode == Keys.W || e.KeyCode == Keys.S || e.KeyCode == Keys.A || e.KeyCode == Keys.D) mario.canMove = true;
        }

        private void ReversePlayerMove(Keys keycode)
        {
            switch (keycode)
            {
                case Keys.Up: luigi.y += 30;
                    break;
                case Keys.Left: luigi.x += 30;
                    break;
                case Keys.Right: luigi.x -= 30;
                    break;
                case Keys.Down: luigi.y -= 30;
                    break;
                case Keys.W: mario.y += 30;
                    break;
                case Keys.A: mario.x += 30;
                    break;
                case Keys.S: mario.y -= 30;
                    break;
                case Keys.D: mario.x -= 30;
                    break;
            }
        }

        private void Motor(object sender, EventArgs g)
        {
            if (boo.mv < 0)
            {
                if (collisionBoxes[boo.y / 30, boo.x / 30].Name == "unpassable") //boo Richtungswechsel links nach rechts
                {
                    boo.mv *= -1;
                    boo.icon.RotateFlip(RotateFlipType.RotateNoneFlipX);
                }
            }
            if (boo.mv > 0)
            {
                if (collisionBoxes[boo.y  / 30, (boo.x + 30) / 30].Name == "unpassable") //boo Richtungswechsel von rechts nach links
                {
                    boo.mv *= -1;
                    boo.icon.RotateFlip(RotateFlipType.RotateNoneFlipX);
                }
            }
            if (ghost1.mv < 0)
            {
                if (collisionBoxes[ghost1.y / 30, ghost1.x / 30].Name == "unpassable") //ghost1 Richtungswechsel links nach rechts
                {
                    ghost1.mv *= -1;
                    ghost1.icon.RotateFlip(RotateFlipType.RotateNoneFlipX);
                }
            }
            if (ghost1.mv > 0)
            {
                if (collisionBoxes[ghost1.y / 30, (ghost1.x + 30)  / 30].Name == "unpassable") //ghost1 Richtungswechsel rechts nach links
                {
                    ghost1.mv *= -1;
                    ghost1.icon.RotateFlip(RotateFlipType.RotateNoneFlipX);
                }
            }

            boo.x += boo.mv;
            ghost1.x += ghost1.mv;

            //Kollision Spieler Geister //?Speicher zu viel?
            Rectangle recMario = new Rectangle(mario.x, mario.y, 30, 30);
            Rectangle recLuigi = new Rectangle(luigi.x, luigi.y, 30, 30);
            Rectangle recBoo = new Rectangle(boo.x, boo.y, 30, 30);
            Rectangle recGhost1 = new Rectangle(ghost1.x, ghost1.y, 30, 30);
            if (recMario.IntersectsWith(recBoo) || 
                recLuigi.IntersectsWith(recBoo) || 
                recMario.IntersectsWith(recGhost1) || 
                recLuigi.IntersectsWith(recGhost1) )
            {
                gameOver = true;
                //Application.Restart();
            }

            canvas.Refresh();
        }

        private void MakeMap()
        {
            for (int x = 0; x < 20; x++)
            {
                for (int y = 0; y < 25; y++)
                {
                    for (int blockX = 0; blockX < 30; blockX++)
                    {
                        for (int blockY = 0; blockY < 30; blockY++)
                        {
                            // ???????
                            mapMaker.SetPixel(x, y, area.background.GetPixel(x * 30 + blockX, y * 30 + blockY));
                        }
                    }
                }
            }
        }
        private void LoadArea() //Neuen Bereich laden
        {
            canvas.Image = area.background; //Hintergrund des neuen Bereichs laden

            for (int y = 0; y < 25; y++) //Wände und Pfade aus Hintergrundbild herauslesen
            {
                for (int x = 0; x < 20; x++)
                {
                    if (area.background.GetPixel(x * 30, y * 30) == Color.FromArgb(127, 83, 44)) //Podzol-Farbe
                    {
                        collisionBoxes[y, x].BackColor = Color.White; //Debug-Farbe
                        collisionBoxes[y, x].Name = "passable";
                    }
                    else
                    {
                        collisionBoxes[y, x].BackColor = Color.Green; //Debug-Farbe
                        collisionBoxes[y, x].Name = "unpassable";
                    }
                }
            }
        }

        private void ShowCoordinates(object sender, MouseEventArgs e)
        {
            if (bKeyPressed)
            {
                MessageBox.Show("x: " + (PointToClient(Cursor.Position).X / 30) + ", y: " + (PointToClient(Cursor.Position).Y / 30).ToString());
                bKeyPressed = false;
            }
        }
        
        Bitmap save = new Bitmap(600, 750);
        private void LevelMaker(object sender, MouseEventArgs e)
        {
            PictureBox senderPb = sender as PictureBox;
            if (e.Button == MouseButtons.Left)
            {
                senderPb.Capture = false;
                senderPb.Image = textureArray[textureIndex];

                for (int x = 0; x < 30; x++)
                {
                    for (int y = 0; y < 30; y++)
                    {
                        save.SetPixel(senderPb.Left + x, senderPb.Top + y, textureArray[textureIndex].GetPixel(x, y));
                    }
                }
            }
            if (e.Button == MouseButtons.Right)
            {
                senderPb.Capture = false;
                senderPb.Image = null;

                for (int x = 0; x < 30; x++)
                {
                    for (int y = 0; y < 30; y++)
                    {
                        save.SetPixel(senderPb.Left + x, senderPb.Top + y, Color.White);
                    }
                }
            }
        }

        private void DeveloperMode()
        {
            if (!developerMode)
            {
                developerMode = true;
                this.Controls.SetChildIndex(canvas, 1); //canvas in Hintergrund
                this.Controls.SetChildIndex(collisionBoxesContainer, 0); //collisionBoxes in Vordergrund
            }
            else
            {
                developerMode = false;
                this.Controls.SetChildIndex(canvas, 0); //canvas in Vordergrund
                this.Controls.SetChildIndex(collisionBoxesContainer, 1); //collisionBoxes in Hintergrund
            }
        }

    }
}
