namespace SnapNoteAdvanced
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
            InitializeSettings();
            BG_AlphaChannel.ValueChanged += OnValueUpdate;
            BG_RedChannel.ValueChanged += OnValueUpdate;
            BG_GreenChannel.ValueChanged += OnValueUpdate;
            BG_BlueChannel.ValueChanged += OnValueUpdate;

            FG_AlphaChannel.ValueChanged += OnValueUpdate;
            FG_RedChannel.ValueChanged += OnValueUpdate;
            FG_GreenChannel.ValueChanged += OnValueUpdate;
            FG_BlueChannel.ValueChanged += OnValueUpdate;
        }

        public List<string> optionsSettings = [];

        private void OnValueUpdate(object? sender, EventArgs e)
        {
            File.WriteAllText("config.txt",
                $"""
                bg:{BG_AlphaChannel.Value},{BG_RedChannel.Value},{BG_GreenChannel.Value},{BG_BlueChannel.Value}
                fg:{FG_AlphaChannel.Value},{FG_RedChannel.Value},{FG_GreenChannel.Value},{FG_BlueChannel.Value}
                """
                );
            panel1.BackColor = Color.FromArgb(
                    BG_AlphaChannel.Value,
                    BG_RedChannel.Value,
                    BG_GreenChannel.Value,
                    BG_BlueChannel.Value);
            panel2.BackColor = Color.FromArgb(
                    FG_AlphaChannel.Value,
                    FG_RedChannel.Value,
                    FG_GreenChannel.Value,
                    FG_BlueChannel.Value);
        }

        private void InitializeSettings()
        {
            try
            {
                optionsSettings = File.ReadAllLines("config.txt").ToList<string>();
                foreach (string s in optionsSettings)
                {
                    string s_val = s.Split(":")[1];
                    if (s.StartsWith("bg:"))
                    {
                        BG_AlphaChannel.Value = Convert.ToInt32(s_val.Split(",")[0]);
                        BG_RedChannel.Value = Convert.ToInt32(s_val.Split(",")[1]);
                        BG_GreenChannel.Value = Convert.ToInt32(s_val.Split(",")[2]);
                        BG_BlueChannel.Value = Convert.ToInt32(s_val.Split(",")[3]);
                    }
                    else if (s.StartsWith("fg:"))
                    {
                        FG_AlphaChannel.Value = Convert.ToInt32(s_val.Split(",")[0]);
                        FG_RedChannel.Value = Convert.ToInt32(s_val.Split(",")[1]);
                        FG_GreenChannel.Value = Convert.ToInt32(s_val.Split(",")[2]);
                        FG_BlueChannel.Value = Convert.ToInt32(s_val.Split(",")[3]);
                    }
                }
                panel1.BackColor = Color.FromArgb(
                    BG_AlphaChannel.Value,
                    BG_RedChannel.Value,
                    BG_GreenChannel.Value,
                    BG_BlueChannel.Value);
                panel2.BackColor = Color.FromArgb(
                    FG_AlphaChannel.Value,
                    FG_RedChannel.Value,
                    FG_GreenChannel.Value,
                    FG_BlueChannel.Value);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to read options.txt due to the following exception: {Environment.NewLine}{ex.Message}");
            }
        }
    }
}
