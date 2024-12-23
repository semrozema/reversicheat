namespace reversicheat
{
    public partial class Form1 : Form
    {
        // Member variables
        const int RED = 0;
        const int BLUE = 1;
        const int NONE = 2;

        int gridsize = 6; // Number of rows and columns
        int[,] grid;
        bool turn = true;
        int cellSize = 50; // Size of each grid cell in pixels
        bool showMoves = false;
        List<(int row, int col)> validMoves = new List<(int row, int col)>();
        public Form1()
        {
            InitializeComponent();
            grid = new int[gridsize, gridsize];

            // Initialize the grid to NONE
            for (int row = 0; row < gridsize; row++)
            {
                for (int col = 0; col < gridsize; col++)
                {
                    grid[row, col] = NONE;
                }
            }

            // Place starting pieces (for a standard Reversi board)
            int mid = gridsize / 2;
            grid[mid - 1, mid - 1] = BLUE;
            grid[mid, mid] = BLUE;
            grid[mid - 1, mid] = RED;
            grid[mid, mid - 1] = RED;

            // Attach event handlers
            panel1.Paint += Panel1_Paint;
            panel1.MouseClick += Panel1_MouseClick;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            panel1.Width = gridsize * cellSize + 1;
            panel1.Height = gridsize * cellSize + 1;
        }

        private void Panel1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // Draw the grid lines
            for (int i = 0; i <= gridsize; i++)
            {
                // Vertical lines
                g.DrawLine(Pens.Black, i * cellSize, 0, i * cellSize, gridsize * cellSize);
                // Horizontal lines
                g.DrawLine(Pens.Black, 0, i * cellSize, gridsize * cellSize, i * cellSize);
            }

            // Draw the pieces
            for (int row = 0; row < gridsize; row++)
            {
                for (int col = 0; col < gridsize; col++)
                {
                    if (grid[row, col] == RED)
                        DrawPiece(g, row, col, Brushes.Red);
                    else if (grid[row, col] == BLUE)
                        DrawPiece(g, row, col, Brushes.Blue);
                }
            }

            if (showMoves)
            {
                using (Pen pen = new Pen(Color.Black, 2))
                {
                    foreach (var (r, c) in validMoves)
                    {
                        int x = c * cellSize + cellSize / 4;
                        int y = r * cellSize + cellSize / 4;
                        int diameter = cellSize / 2;
                        g.DrawEllipse(pen, x, y, diameter, diameter);
                    }
                }
            }
        }

        private void DrawPiece(Graphics g, int row, int col, Brush color)
        {
            // Position and diameter of the piece
            int x = col * cellSize + cellSize / 4;
            int y = row * cellSize + cellSize / 4;
            int diameter = cellSize / 2;

            g.FillEllipse(color, x, y, diameter, diameter);
        }

        /// <summary>
        /// Check if the specified (row, col) is on the board.
        /// </summary>
        private bool IsOnBoard(int row, int col)
        {
            return row >= 0 && row < gridsize && col >= 0 && col < gridsize;
        }

        /// <summary>
        /// Checks if placing 'color' at (row, col) is a valid Reversi move.
        /// If 'flipPieces' is true, actually flip the captured pieces.
        /// Returns true if at least one direction flips at least one piece; false otherwise.
        /// </summary>
        private bool CheckAndFlip(int row, int col, int color, bool flipPieces)
        {
            // The opposing color to look for
            int opponent = (color == BLUE) ? RED : BLUE;
            bool validMove = false;

            // Check all 8 directions using nested loops
            for (int dRow = -1; dRow <= 1; dRow++)
            {
                for (int dCol = -1; dCol <= 1; dCol++)
                {
                    // Skip the (0,0) direction because that's not a move
                    if (dRow == 0 && dCol == 0)
                        continue;

                    int r = row + dRow;
                    int c = col + dCol;
                    int countOpponent = 0;

                    // Move while we see the opponent's color
                    while (IsOnBoard(r, c) && grid[r, c] == opponent)
                    {
                        r += dRow;
                        c += dCol;
                        countOpponent++;
                    }

                    // If we collected one or more opponent pieces AND
                    // ended on the current player's color => there's a flippable line
                    if (countOpponent > 0 &&
                        IsOnBoard(r, c) && grid[r, c] == color)
                    {
                        validMove = true;

                        // If flipPieces is true, flip them
                        if (flipPieces)
                        {
                            // Roll back one step in that direction
                            r -= dRow;
                            c -= dCol;

                            // Flip all in-between opponent pieces
                            while ((r != row) || (c != col))
                            {
                                grid[r, c] = color;
                                r -= dRow;
                                c -= dCol;
                            }
                        }
                    }
                }
            }

            return validMove;
        }
        /// <summary>
        /// Gets all valid moves for the given color
        /// and stores them in validMoves.
        /// </summary>
        private void ComputeValidMoves(int color)
        {
            validMoves.Clear(); // start fresh

            for (int row = 0; row < gridsize; row++)
            {
                for (int col = 0; col < gridsize; col++)
                {
                    // Must be an empty cell
                    if (grid[row, col] == NONE)
                    {
                        // If placing here flips anything, it's a valid move
                        bool canFlip = CheckAndFlip(row, col, color, flipPieces: false);
                        if (canFlip)
                        {
                            validMoves.Add((row, col));
                        }
                    }
                }
            }
        }

        private void Panel1_MouseClick(object sender, MouseEventArgs e)
        {
            int col = e.X / cellSize;
            int row = e.Y / cellSize;

            if (!IsOnBoard(row, col)) return;

            // Check if cell is empty
            if (grid[row, col] != NONE) return;

            // Determine which color is playing
            int currentColor = turn ? BLUE : RED;

            // Check if placing currentColor at (row, col) would flip any pieces
            bool canFlip = CheckAndFlip(row, col, currentColor, flipPieces: false);
            if (canFlip)
            {
                // Actually flip
                CheckAndFlip(row, col, currentColor, flipPieces: true);

                // Place the current piece
                grid[row, col] = currentColor;

                // Switch turns
                turn = !turn;

                // Recompute valid moves for the next player
                if (showMoves)
                {
                    int nextColor = turn ? BLUE : RED;
                    ComputeValidMoves(nextColor);
                }

                // Redraw
                panel1.Invalidate();
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            // Toggle the show/hide
            showMoves = !showMoves;

            if (showMoves)
            {
                // Determine current color (turn == true => BLUE, false => RED)
                int currentColor = turn ? BLUE : RED;
                // Compute valid moves
                ComputeValidMoves(currentColor);
            }
            else
            {
                // No longer showing moves
                validMoves.Clear();
            }

            // Redraw the panel with or without the black circles
            panel1.Invalidate();
        }



    }
}
