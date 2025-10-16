using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;

namespace TicTacToe
{
    public class TicTacToe : Form
    {
        private char[] board = new char[9];
        private bool xTurn = true;
        private int? pendingIndex = null;

        private Dictionary<Button, int> map = null!;
        private Button[] cells = null!;

        private Button TopLeft = null!, TopMiddle = null!, TopRight = null!;
        private Button MiddleLeft = null!, Middle = null!, MiddleRight = null!;
        private Button BottomLeft = null!, BottomMiddle = null!, BottomRight = null!;
        private Button Start = null!, Submit = null!, CancelBtn = null!;
        private Label Player_Turn = null!;

        private static readonly int[][] Wins = new[]
        {
            new[] {0,1,2}, new[] {3,4,5}, new[] {6,7,8},
            new[] {0,3,6}, new[] {1,4,7}, new[] {2,5,8},
            new[] {0,4,8}, new[] {2,4,6}
        };

        public TicTacToe()
        {
            InitializeComponent();
            InitializeGame();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form properties
            this.ClientSize = new Size(400, 500);
            this.Text = "Tic-Tac-Toe 🎮";
            this.BackColor = Color.SteelBlue;
            this.ForeColor = Color.Black;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Player Turn Label
            Player_Turn = new Label
            {
                Name = "Player_Turn",
                Location = new Point(20, 20),
                Size = new Size(360, 40),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "Click Start to begin!",
                BackColor = Color.Transparent,
                ForeColor = Color.White
            };

            // Grid buttons (3x3)
            int btnSize = 100;
            int startX = 50;
            int startY = 80;
            int spacing = 10;

            TopLeft = CreateGridButton(startX, startY, btnSize);
            TopMiddle = CreateGridButton(startX + btnSize + spacing, startY, btnSize);
            TopRight = CreateGridButton(startX + 2 * (btnSize + spacing), startY, btnSize);

            MiddleLeft = CreateGridButton(startX, startY + btnSize + spacing, btnSize);
            Middle = CreateGridButton(startX + btnSize + spacing, startY + btnSize + spacing, btnSize);
            MiddleRight = CreateGridButton(startX + 2 * (btnSize + spacing), startY + btnSize + spacing, btnSize);

            BottomLeft = CreateGridButton(startX, startY + 2 * (btnSize + spacing), btnSize);
            BottomMiddle = CreateGridButton(startX + btnSize + spacing, startY + 2 * (btnSize + spacing), btnSize);
            BottomRight = CreateGridButton(startX + 2 * (btnSize + spacing), startY + 2 * (btnSize + spacing), btnSize);

            // Control buttons
            Start = new Button
            {
                Text = "Start Game",
                Location = new Point(50, 420),
                Size = new Size(90, 40),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.LightGreen,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            Start.FlatAppearance.BorderSize = 0;
            Start.Click += Start_Click;

            Submit = new Button
            {
                Text = "Submit",
                Location = new Point(155, 420),
                Size = new Size(90, 40),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.LightBlue,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            Submit.FlatAppearance.BorderSize = 0;
            Submit.Click += Submit_Click;

            CancelBtn = new Button
            {
                Text = "Exit",
                Location = new Point(260, 420),
                Size = new Size(90, 40),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.LightCoral,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            CancelBtn.FlatAppearance.BorderSize = 0;
            CancelBtn.Click += Cancel_Click;

            // Add controls to form
            this.Controls.AddRange(new Control[] {
                Player_Turn,
                TopLeft, TopMiddle, TopRight,
                MiddleLeft, Middle, MiddleRight,
                BottomLeft, BottomMiddle, BottomRight,
                Start, Submit, CancelBtn
            });

            this.ResumeLayout(false);
        }

        private Button CreateGridButton(int x, int y, int size)
        {
            var btn = new Button
            {
                Location = new Point(x, y),
                Size = new Size(size, size),
                Font = new Font("Arial", 36, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.AliceBlue,
                ForeColor = SystemColors.ControlText,
                Text = "",
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 2;
            btn.FlatAppearance.BorderColor = Color.DarkSlateGray;
            btn.Click += GridButton_Click;
            return btn;
        }

        private void InitializeGame()
        {
            cells = new[]
            {
                TopLeft, TopMiddle, TopRight,
                MiddleLeft, Middle, MiddleRight,
                BottomLeft, BottomMiddle, BottomRight
            };

            map = new Dictionary<Button, int>
            {
                {TopLeft, 0}, {TopMiddle, 1}, {TopRight, 2},
                {MiddleLeft, 3}, {Middle, 4}, {MiddleRight, 5},
                {BottomLeft, 6}, {BottomMiddle, 7}, {BottomRight, 8}
            };

            SetPlayEnabled(false);
        }

        private void GridButton_Click(object? sender, EventArgs e)
        {
            if (sender is not Button btn) return;
            if (!map.TryGetValue(btn, out int idx)) return;
            PreviewCell(idx);
        }

        private void Cancel_Click(object? sender, EventArgs e)
        {
            Close();
        }

        private void Submit_Click(object? sender, EventArgs e) => SubmitMove();

        private void Start_Click(object? sender, EventArgs e) => StartGame();

        private void StartGame()
        {
            for (int i = 0; i < board.Length; i++) board[i] = '\0';
            xTurn = true;
            pendingIndex = null;

            foreach (var b in cells)
            {
                b.Text = "";
                b.Enabled = true;
                b.ForeColor = SystemColors.ControlText;
                b.BackColor = Color.AliceBlue;
            }

            SetPlayEnabled(true);
            UpdateTurnLabel();
        }

        private void SetPlayEnabled(bool enabled)
        {
            for (int i = 0; i < cells.Length; i++)
                cells[i].Enabled = enabled && board[i] == '\0';

            Submit.Enabled = enabled;
        }

        private void PreviewCell(int idx)
        {
            if (board[idx] != '\0') return;

            if (pendingIndex.HasValue) ClearPreviewVisual(pendingIndex.Value);

            pendingIndex = idx;

            var mark = xTurn ? 'X' : 'O';
            cells[idx].Text = mark.ToString();
            cells[idx].ForeColor = Color.Gray;
        }

        private void ClearPreviewVisual(int idx)
        {
            if (board[idx] == '\0')
            {
                cells[idx].Text = "";
                cells[idx].ForeColor = SystemColors.ControlText;
                cells[idx].BackColor = Color.AliceBlue;
            }
        }

        private void SubmitMove()
        {
            if (!pendingIndex.HasValue) return;

            int idx = pendingIndex.Value;
            if (board[idx] != '\0') return;

            char mark = xTurn ? 'X' : 'O';
            board[idx] = mark;

            cells[idx].Text = mark.ToString();
            cells[idx].ForeColor = mark == 'X' ? Color.DarkBlue : Color.DarkRed;
            cells[idx].Enabled = false;
            cells[idx].BackColor = Color.White;

            pendingIndex = null;

            if (IsWin(mark))
            {
                EndGame($"{mark} wins!", true);
                return;
            }
            if (IsDraw())
            {
                EndGame("Draw!", false);
                return;
            }

            xTurn = !xTurn;
            UpdateTurnLabel();

            for (int i = 0; i < cells.Length; i++)
                cells[i].Enabled = board[i] == '\0';
        }

        private bool IsWin(char mark)
        {
            foreach (var w in Wins)
                if (w.All(i => board[i] == mark)) return true;
            return false;
        }

        private bool IsDraw() => board.All(c => c != '\0');

        private void EndGame(string message, bool isWin)
        {
            foreach (var b in cells) b.Enabled = false;
            Submit.Enabled = false;

            // Show emoji celebration for wins!
            if (isWin)
            {
                string emoji = "🎉🏆✨";
                MessageBox.Show($"{emoji}\n\n{message}\n\n{emoji}",
                    "Victory!",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.None);
            }
            else
            {
                MessageBox.Show(message, "Game Over", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            Player_Turn.Text = $"{message} Click Start to play again.";
        }

        private void UpdateTurnLabel()
        {
            Player_Turn.Text = xTurn ? "X to move — pick a square, then Submit."
                                     : "O to move — pick a square, then Submit.";
        }
    }
}
