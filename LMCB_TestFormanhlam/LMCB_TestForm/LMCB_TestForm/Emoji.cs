using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LMCB_TestForm
{
    public partial class Emoji : Form
    {
        public string SelectedEmoji { get; private set; }
        public Emoji()
        {
            InitializeComponent();
        }
        private readonly Dictionary<string, string> emojiDictionary = new Dictionary<string, string>
        {
            { "Smile", "😊" },
            { "Heart", "❤️" },
            { "Thumbs Up", "👍" },
            { "Wink", "😉" },
            { "Laugh", "😂" },
        };
        private void LoadEmojiList()
        {
            foreach (var emojiPair in emojiDictionary)
            {
                listBox1.Items.Add(emojiPair.Key);
            }
        }

        private void Emoji_Load(object sender, EventArgs e)
        {
            LoadEmojiList();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                SelectedEmoji = emojiDictionary[listBox1.SelectedItem.ToString()];
                DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một emoji.", "Chọn Emoji", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
