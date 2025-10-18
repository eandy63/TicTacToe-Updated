using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
/*Program is Tic-Tac-Toe game. The game begins by clicking start button. 
 * Player X goes first by clicking one of the 9 buttons and then clicking submit.
 Player O goes next. Winner is declared when one player is able to get three
in a row or game ends in a draw if no player gets three in a row.*/


namespace TicTacToe
{
    // The main form class for the Tic-Tac-Toe game.
    public class TicTacToe : Form
    {
        // --- Game State Variables ---
        private char[] board = new char[9]; // Represents the 3x3 board state. '\0' means empty.
        private bool xTurn = true; // True if it's Player X's turn, false for Player O.
        private int? pendingIndex = null; // Index of the cell currently previewed, waiting for 'Submit'.
        private int xWins = 0; // Tracks the score for Player X.
        private int oWins = 0; // Tracks the score for Player O.

        // --- UI Control Maps and Arrays ---
        private Dictionary<Button, int> map = null!; // Maps each grid Button object to its index (0-8) in the 'board' array.
        private Button[] cells = null!; // An array of the 9 grid buttons for easy iteration.

        // --- UI Control Declarations ---
        // 9 grid buttons
        private Button TopLeft = null!, TopMiddle = null!, TopRight = null!;
        private Button MiddleLeft = null!, Middle = null!, MiddleRight = null!;
        private Button BottomLeft = null!, BottomMiddle = null!, BottomRight = null!;

        // Control buttons
        private Button Start = null!, Submit = null!, CancelBtn = null!;

        // Information labels
        private Label Player_Turn = null!; // Displays whose turn it is or the game outcome.
        private Label SpotTakenMessage = null!; // NEW: Displays temporary messages like "Spot is taken".

        // --- Winning Combinations ---
        // A list of all 8 possible ways to win (3 horizontal, 3 vertical, 2 diagonal).
        private static readonly int[][] Wins = new[]
        {
            new[] {0,1,2}, new[] {3,4,5}, new[] {6,7,8},
            new[] {0,3,6}, new[] {1,4,7}, new[] {2,5,8},
            new[] {0,4,8}, new[] {2,4,6}
        };

        // Constructor
        public TicTacToe()
        {
            InitializeComponent();
            InitializeGame();
        }

        // Sets up all UI elements (Form, Labels, Buttons, and Menu)
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

            // --- Menu Strip (Task 2) ---
            MenuStrip menuStrip = new MenuStrip();
            ToolStripMenuItem gameMenuItem = new ToolStripMenuItem("Game");
            ToolStripMenuItem aboutMenuItem = new ToolStripMenuItem("About");
            ToolStripMenuItem scoreTrackerItem = new ToolStripMenuItem("Score Tracker");

            menuStrip.BackColor = Color.LightSlateGray;
            menuStrip.ForeColor = Color.White;
            menuStrip.Font = new Font("Segoe UI", 9, FontStyle.Bold);

            // Structure the menu
            gameMenuItem.DropDownItems.Add(aboutMenuItem);
            gameMenuItem.DropDownItems.Add(scoreTrackerItem);
            menuStrip.Items.Add(gameMenuItem);

            // Add handlers for menu items
            aboutMenuItem.Click += AboutMenuItem_Click;
            scoreTrackerItem.Click += ScoreTrackerItem_Click;

            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);

            // Player Turn Label (Fixed size for Task 3)
            Player_Turn = new Label
            {
                Name = "Player_Turn",
                Location = new Point(10, 35), // Adjusted position slightly
                Size = new Size(380, 40),     // INCREASED WIDTH to prevent 'Submit' cutoff (Fix for Task 3)
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "Click Start to begin!",
                BackColor = Color.Transparent,
                ForeColor = Color.White
            };

            // Spot Taken Message Label (Task 1)
            SpotTakenMessage = new Label
            {
                Name = "SpotTakenMessage",
                Location = new Point(10, 68), // Placed below Player_Turn
                Size = new Size(380, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "",
                BackColor = Color.Transparent,
                ForeColor = Color.Yellow
            };

            // Grid buttons (3x3) layout calculations
            int btnSize = 100;
            int startX = 50;
            int startY = 90; // Moved grid down to accommodate menu and new label
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

            // Control buttons (Adjusted positions for better spacing)
            int buttonWidth = 100;
            int buttonHeight = 40;
            int buttonY = 440;
            int buttonSpacing = (400 - (buttonWidth * 3)) / 4; // Auto-calculate spacing

            Start = new Button
            {
                Text = "Start Game",
                Location = new Point(buttonSpacing, buttonY),
                Size = new Size(buttonWidth, buttonHeight),
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
                Location = new Point(buttonSpacing * 2 + buttonWidth, buttonY),
                Size = new Size(buttonWidth, buttonHeight),
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
                Location = new Point(buttonSpacing * 3 + buttonWidth * 2, buttonY),
                Size = new Size(buttonWidth, buttonHeight),
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
                SpotTakenMessage, // Added new message label
                TopLeft, TopMiddle, TopRight,
                MiddleLeft, Middle, MiddleRight,
                BottomLeft, BottomMiddle, BottomRight,
                Start, Submit, CancelBtn
            });

            this.ResumeLayout(false);
            this.PerformLayout(); // Ensure the menu strip is properly laid out
        }

        // Helper function to create the 9 grid buttons with standard properties and event handler.
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
            btn.Click += GridButton_Click; // Attaches the click event handler
            return btn;
        }

        // Initializes game maps and disables play controls until 'Start' is clicked.
        private void InitializeGame()
        {
            // Populate the cells array with the button references
            cells = new[]
            {
                TopLeft, TopMiddle, TopRight,
                MiddleLeft, Middle, MiddleRight,
                BottomLeft, BottomMiddle, BottomRight
            };

            // Populate the map dictionary for quick lookup of cell index
            map = new Dictionary<Button, int>
            {
                {TopLeft, 0}, {TopMiddle, 1}, {TopRight, 2},
                {MiddleLeft, 3}, {Middle, 4}, {MiddleRight, 5},
                {BottomLeft, 6}, {BottomMiddle, 7}, {BottomRight, 8}
            };

            SetPlayEnabled(false); // Disable game buttons initially
        }

        // Displays a temporary message in the SpotTakenMessage label.
        // Used for notifications that disappear after 2 seconds.
        private void ShowTempMessage(string message)
        {
            SpotTakenMessage.Text = message;

            // Use a WinForms Timer for UI thread-safe delayed actions
            var timer = new System.Windows.Forms.Timer();
            timer.Interval = 2000;
            timer.Tick += (s, ev) =>
            {
                if (SpotTakenMessage.Text == message)
                {
                    SpotTakenMessage.Text = "";
                }
                timer.Stop();
                timer.Dispose();
            };
            timer.Start();
        }

        // Clears the temporary message label.
        private void ClearTempMessage()
        {
            SpotTakenMessage.Text = "";
        }

        // Event handler for all 9 grid buttons.
        private void GridButton_Click(object? sender, EventArgs e)
        {
            if (sender is not Button btn) return;
            if (!map.TryGetValue(btn, out int idx)) return;

            PreviewCell(idx);
        }

        // Event handler for the Exit button.
        private void Cancel_Click(object? sender, EventArgs e)
        {
            Close(); // Closes the application form
        }

        // Event handler for the Submit button.
        private void Submit_Click(object? sender, EventArgs e) => SubmitMove();

        // Event handler for the Start Game button.
        private void Start_Click(object? sender, EventArgs e) => StartGame();

        // Event handler for the About menu item (Task 2).
        private void AboutMenuItem_Click(object? sender, EventArgs e)
        {
            MessageBox.Show(
                "Welcome to Tic-Tac-Toe! The game starts with Player X, who marks a square. " +
                "Players alternate turns until one player successfully places three of their marks " +
                "in a horizontal, vertical, or diagonal row (a win), or all nine squares are filled " +
                "without a winner (a draw). Click 'Start Game' to begin or reset the board. Use the 'Submit' button to finalize your move.",
                "About Tic-Tac-Toe",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        // Event handler for the Score Tracker menu item (Task 2).
        private void ScoreTrackerItem_Click(object? sender, EventArgs e)
        {
            MessageBox.Show(
                $"Current Score:\n\nPlayer X Wins: {xWins}\nPlayer O Wins: {oWins}",
                "Score Tracker",
                MessageBoxButtons.OK,
                MessageBoxIcon.None);
        }

        // Resets the game state and UI to start a new game.
        private void StartGame()
        {
            // Clear the board array
            for (int i = 0; i < board.Length; i++) board[i] = '\0';
            xTurn = true;
            pendingIndex = null;
            ClearTempMessage();

            // Reset all cell button visuals and enable state
            foreach (var b in cells)
            {
                b.Text = "";
                b.Enabled = true;
                b.ForeColor = SystemColors.ControlText;
                b.BackColor = Color.AliceBlue;
            }

            SetPlayEnabled(true); // Enable grid and Submit button
            UpdateTurnLabel(); // Set the turn label to Player X
        }

        // Controls the enabled state of the grid buttons and the Submit button.
        private void SetPlayEnabled(bool enabled)
        {
            for (int i = 0; i < cells.Length; i++)
                // Only enable cells that are currently empty on the board
                cells[i].Enabled = enabled && board[i] == '\0';

            Submit.Enabled = enabled;
        }

        // Previews the player's mark in the selected cell without committing the move.
        private void PreviewCell(int idx)
        {
            // Task 1: Check if the spot is already taken
            if (board[idx] != '\0')
            {
                // Display the "Spot is taken" message and stop processing the click
                ShowTempMessage("Spot is taken, please choose another square.");
                return;
            }

            ClearTempMessage(); // Clear any spot taken message if the click was valid

            // If a different cell was previously selected, clear its preview visual
            if (pendingIndex.HasValue) ClearPreviewVisual(pendingIndex.Value);

            pendingIndex = idx; // Set the new pending index

            var mark = xTurn ? 'X' : 'O';
            cells[idx].Text = mark.ToString();
            cells[idx].ForeColor = Color.Gray; // Show the preview mark in a lighter color
        }

        // Clears the temporary preview visual from a cell.
        private void ClearPreviewVisual(int idx)
        {
            // Only clear if the board position is actually empty (i.e., it was just a preview)
            if (board[idx] == '\0')
            {
                cells[idx].Text = "";
                cells[idx].ForeColor = SystemColors.ControlText;
                cells[idx].BackColor = Color.AliceBlue;
            }
        }

        // Commits the currently previewed move to the board.
        private void SubmitMove()
        {
            // Check if a cell has been selected
            if (!pendingIndex.HasValue)
            {
                ShowTempMessage("Please select a square before submitting.");
                return;
            }

            int idx = pendingIndex.Value;

            // Double-check if the spot is taken (shouldn't happen if PreviewCell works, but for safety)
            if (board[idx] != '\0')
            {
                ShowTempMessage("Error: Spot is taken.");
                pendingIndex = null; // Clear pending move
                return;
            }

            ClearTempMessage();

            char mark = xTurn ? 'X' : 'O';
            board[idx] = mark; // Commit the move to the game board state

            // Update cell visual to permanent mark
            cells[idx].Text = mark.ToString();
            cells[idx].ForeColor = mark == 'X' ? Color.DarkBlue : Color.DarkRed;
            cells[idx].Enabled = false; // Disable the cell after it's played
            cells[idx].BackColor = Color.White;

            pendingIndex = null; // Clear the pending selection

            // Check for game end conditions
            if (IsWin(mark))
            {
                EndGame($"{mark} wins!", true, mark); // Pass the winning mark
                return;
            }
            if (IsDraw())
            {
                EndGame("Draw!", false);
                return;
            }

            // If game continues, switch turn
            xTurn = !xTurn;
            UpdateTurnLabel(); // Update the turn label

            // Re-enable only the empty cells for the next player
            for (int i = 0; i < cells.Length; i++)
                cells[i].Enabled = board[i] == '\0';
        }

        // Checks if the given mark has achieved a winning combination.
        private bool IsWin(char mark)
        {
            // Iterate through all possible winning patterns
            foreach (var w in Wins)
                // Check if all 3 cells in the pattern match the current player's mark
                if (w.All(i => board[i] == mark)) return true;
            return false;
        }

        // Checks if the board is full (resulting in a draw).
        private bool IsDraw() => board.All(c => c != '\0');

        // Handles the end of the game (Win or Draw).
        private void EndGame(string message, bool isWin, char winner = '\0')
        {
            // Disable all interactive elements
            foreach (var b in cells) b.Enabled = false;
            Submit.Enabled = false;

            // Update score if there is a winner
            if (isWin)
            {
                if (winner == 'X') xWins++;
                else if (winner == 'O') oWins++;

                // Show emoji celebration for wins!
                string emoji = "🎉🏆✨";
                MessageBox.Show($"{emoji}\n\n{message}\n\n{emoji}",
                    "Victory!",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.None);
            }
            else // Draw case
            {
                MessageBox.Show(message, "Game Over", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            // Update the main label with the result and current scores.
            Player_Turn.Text = $"{message} Click Start to play again. X: {xWins} | O: {oWins}";
        }

        // Updates the label to show whose turn it is. (Line 387 in original logic)
        private void UpdateTurnLabel()
        {
            // This label text is now shorter and the label size was increased in InitializeComponent,
            // which resolves the 'Submit' cutoff issue (Task 3).
            Player_Turn.Text = xTurn ? "X's turn. Pick a square, then Submit."
                                     : "O's turn. Pick a square, then Submit.";
        }
    }
}
