using Raylib_cs;

namespace RaylibTetris
{
    internal class Program
    {
        static void Main(string[] args)
        {
            GameManager.Run();
        }
    }

    static class GameManager
    {
        public static int screenSizeX = 600;
        public static int screenSizeY = 750;

        public static Board board = new Board();
        public static int boardScreenX = 32;
        public static int boardScreenY = 6;

        public static Piece playPiece = new Piece(5, 0, 4, 2);
        public static bool playPieceCanDescend = true;
        public static List<int> pieceBag = new List<int>();
        public static List<int> futurePieceBag = new List<int>();
        private static List<int> tempBag = new List<int>();
        public static int heldPiece = -1;
        public static bool canSwapHeldPiece = true;

        public static float gravity = 0.05f;
        public static float timeBetweenDrops => 1 / (gravity * targetFps);
        public static bool softDrop = false;
        public static bool hardDrop = false;

        public static float lockWaitTime = 0.5f;
        public static float lockTimer = lockWaitTime;
        public static bool locking = false;

        public static float targetFps = 60;

        public static float dropTimer = timeBetweenDrops;

        public static Color screenBgColor = new Color(48, 75, 89, 255);

        public static Random rand = new Random();

        public static void Run()
        {
            Begin();

            while (!Raylib.WindowShouldClose())
            {
                Update();

                Draw();
            }

            End();
        }

        public static void Begin()
        {
            Raylib.InitWindow(screenSizeX, screenSizeY, "Raylibtris");
            Raylib.SetTargetFPS(60);
            for (int x = 0; x < Board.width; x++)
            {
                int height = rand.Next(2, 7);
                for (int y = 0; y < height; y++)
                {
                    //board.cells[x, Board.height - y - 1] = rand.Next(0, 7);
                }
            }
            InitialisePieceBags();
            ResetPlayPiece();
        }

        public static void Update()
        {
            bool pieceMovedByPlayer = false;

            if (Raylib.IsKeyPressed(KeyboardKey.KEY_LEFT))
            {
                bool moved = CollisionManager.TryMovePiece(board, playPiece, -1, 0);
                pieceMovedByPlayer |= moved;
            }
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_RIGHT))
            {
                bool moved = CollisionManager.TryMovePiece(board, playPiece, 1, 0);
                pieceMovedByPlayer |= moved;
            }
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_DOWN))
            {
                dropTimer = timeBetweenDrops * 0.2f;
            }
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_UP))
            {
                bool rotated = CollisionManager.TryRotatePiece(board, playPiece, true);
                pieceMovedByPlayer |= rotated;
            }
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_P))
            {
                playPiece.type++;
                playPiece.type %= 7;
            }
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_SPACE))
            {
                dropTimer = -100000000;
                hardDrop = true;
            }
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_C))
            {
                if (canSwapHeldPiece)
                {
                    SwapHeldPiece();
                    pieceMovedByPlayer = true;
                }
            }
            softDrop = Raylib.IsKeyDown(KeyboardKey.KEY_DOWN);

            if (pieceMovedByPlayer)
            {
                playPieceCanDescend = CollisionManager.CanMoveDown(board, playPiece);
            }

            if (playPieceCanDescend)
            {
                locking = false;
                dropTimer -= Raylib.GetFrameTime();
                if (dropTimer <= 0)
                {
                    int counter = 0;
                    while (playPieceCanDescend == true && counter < Board.height && dropTimer <= 0)
                    {
                        CollisionManager.TryMovePiece(board, playPiece, 0, 1);
                        playPieceCanDescend = CollisionManager.CanMoveDown(board, playPiece);

                        counter++;
                        dropTimer += timeBetweenDrops * (softDrop ? 0.2f : 1);

                        if (hardDrop && !playPieceCanDescend)
                        {
                            LockPlayPiece();
                            hardDrop = false;
                        }
                    }
                }
            }
            else
            {
                if (locking == false)
                {
                    lockTimer = lockWaitTime;
                    locking = true;
                }

                lockTimer -= Raylib.GetFrameTime();

                if (lockTimer <= 0)
                {
                    LockPlayPiece();
                }
            }
        }

        public static void Draw()
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(screenBgColor);
            BoardRenderer.DrawBoard(board, boardScreenX, boardScreenY);

            BoardRenderer.DrawPieceOnBoard(playPiece, boardScreenX, boardScreenY);

            Raylib.DrawRectangle(boardScreenX, boardScreenY, Board.width * BoardRenderer.gridSize, (int)(0.8f * BoardRenderer.gridSize), screenBgColor);
            DrawHeldPiece();
            DrawPieceBag();
            Raylib.EndDrawing();
        }

        public static void End()
        {
            Raylib.CloseWindow();
        }

        public static void SwapHeldPiece()
        {
            canSwapHeldPiece = false;
            int temp = playPiece.type;
            if (heldPiece == -1)
            {
                List<int> bag = pieceBag.Count > 0 ? pieceBag : futurePieceBag;
                playPiece.type = bag[0];
                bag.RemoveAt(0);
            }
            else
            {
                playPiece.type = heldPiece;
            }
            playPiece.rotation = 0;
            playPiece.positionX = 4;
            playPiece.positionY = 1;
            heldPiece = temp;

        }

        public static void ResetPlayPiece()
        {
            if (pieceBag.Count == 0)
            {
                ResetAllBags();
            }

            playPiece.type = pieceBag[0];
            pieceBag.RemoveAt(0);

            playPiece.rotation = 0;
            playPiece.positionX = 4;
            playPiece.positionY = 1;
        }

        public static void InitialisePieceBags()
        {
            ResetPieceBag(pieceBag);
            ResetPieceBag(futurePieceBag);
        }

        public static void ResetAllBags()
        {
            for (int i = 0; i < futurePieceBag.Count; i++)
            {
                pieceBag.Add(futurePieceBag[i]);
            }
            ResetPieceBag(futurePieceBag);
        }

        public static void ResetPieceBag(List<int> targetBag)
        {
            tempBag.Clear();
            for (int i = 0; i < 7; i++)
            {
                tempBag.Add(i);
            }

            targetBag.Clear();
            for (int i = 0; i < 7; i++)
            {
                int randomIndex = rand.Next(0, tempBag.Count);
                targetBag.Add(tempBag[randomIndex]);
                tempBag.RemoveAt(randomIndex);
            }
        }

        public static void DrawPieceBag()
        {
            const int ox = 430;
            const int oy = 230;

            Raylib.DrawRectangle(ox - 60, oy - 60, 150, 390, Color.BLACK);

            for (int i = 0; i < pieceBag.Count + futurePieceBag.Count && i < 4; i++)
            {
                int pieceType = i < pieceBag.Count ? pieceBag[i] : futurePieceBag[i - pieceBag.Count];
                int mx = pieceType == 0 || pieceType == 3 ? -15 : 0;
                int my = pieceType == 0 ? -15 : 0;
                BoardRenderer.DrawPiece(new Piece(pieceType, 0, 0, 0), ox + mx, i * 90 + oy + my, 30);

            }
        }

        public static void DrawHeldPiece()
        {
            const int ox = 430;
            const int oy = 92;

            Raylib.DrawRectangle(ox - 60, oy - 60, 150, 120, Color.BLACK);

            int mx = heldPiece == 0 || heldPiece == 3 ? -15 : 0;
            int my = heldPiece == 0 ? -15 : 0;
            if (heldPiece != -1) BoardRenderer.DrawPiece(new Piece(heldPiece, 0, 0, 0), ox + mx, oy + my, 30);
        }

        public static void LockPlayPiece()
        {
            int lineClearMin = Board.height;
            int lineClearMax = 0;

            //lock piece
            for (int i = 0; i < 4; i++)
            {
                VecInt2 pos = PieceData.GetRotatedPieceOffset(playPiece.type, playPiece.rotation, i);

                lineClearMin = Math.Min(lineClearMin, pos.y + playPiece.positionY);
                lineClearMax = Math.Max(lineClearMax, pos.y + playPiece.positionY);

                board.cells[playPiece.positionX + pos.x, playPiece.positionY + pos.y] = playPiece.type + 1;
            }

            canSwapHeldPiece = true;

            //generate new piece
            ResetPlayPiece();

            locking = false;
            playPieceCanDescend = true;
            dropTimer = 0;

            //clear lines
            ClearLines(lineClearMin, lineClearMax);
        }

        public static void ClearLines(int min, int max)
        {
            int clearedLines = 0;
            for (int y = max; y >= 0; y--)
            {
                //check if row i is full
                bool full = y >= min;
                int x = 0;
                while (full == true && x < Board.width)
                {
                    if (board.cells[x, y] == 0)
                    {
                        full = false;
                    }
                    x++;
                }
                if (full == true)
                {
                    clearedLines++;
                }
                else if (clearedLines > 0)
                {
                    for (x = 0; x < Board.width; x++)
                    {
                        board.cells[x, y + clearedLines] = board.cells[x, y];
                    }
                }
            }
        }
    }

    static class CollisionManager
    {
        /// <returns>True if piece has moved, false is collision would occur</returns>
        public static bool TryMovePiece(Board board, Piece piece, int deltaX, int deltaY)
        {
            bool moved = false;
            if (CanPieceMoveTo(board, piece, piece.positionX + deltaX, piece.positionY + deltaY))
            {
                piece.positionX += deltaX;
                piece.positionY += deltaY;
                moved = true;
            }
            return moved;
        }

        public static bool CanPieceMoveTo(Board board, Piece piece, int targetX, int targetY)
        {
            bool collisionFound = false;
            int i = 0;
            while (collisionFound == false && i < 4)
            {
                VecInt2 pos = PieceData.GetRotatedPieceOffset(piece.type, piece.rotation, i);
                pos.x += targetX;
                pos.y += targetY;

                if (pos.x < 0 || pos.y < 0 || pos.x >= Board.width || pos.y >= Board.height)
                {
                    collisionFound = true;
                }
                else
                {
                    collisionFound = board.cells[pos.x, pos.y] != 0;
                }

                i++;
            }
            return !collisionFound;
        }

        public static bool CanMoveDown(Board board, Piece piece)
        {
            return CanPieceMoveTo(board, piece, piece.positionX, piece.positionY + 1);
        }

        public static bool TryRotatePiece(Board board, Piece piece, bool clockwise)
        {
            if (piece.type == 3) //square piece
            {
                piece.rotation += clockwise ? -1 : 1;
                if (piece.rotation < 0) piece.rotation += 4;
                else if (piece.rotation >= 4) piece.rotation -= 4;
                return true;
            }

            int targetRotation = piece.rotation;
            if (clockwise)
            {
                targetRotation--;
                if (targetRotation < 0) targetRotation += 4;
            }
            else
            {
                targetRotation++;
                targetRotation %= 4;
            }

            VecInt2[] pieces = new VecInt2[4];
            for (int i = 0; i < 4; i++)
            {
                pieces[i] = PieceData.GetRotatedPieceOffset(piece.type, targetRotation, i);
                pieces[i] += new VecInt2(piece.positionX, piece.positionY);
            }

            VecInt2[] testOffsets = new VecInt2[5];
            int[,] offsetsX = piece.type == 0 ? PieceData.srsOffsetsX_I : PieceData.srsOffsetsX_J;
            int[,] offsetsY = piece.type == 0 ? PieceData.srsOffsetsY_I : PieceData.srsOffsetsY_J;
            for (int i = 0; i < 5; i++)
            {
                testOffsets[i] = new VecInt2(-offsetsX[piece.rotation, i] + offsetsX[targetRotation, i], -offsetsY[piece.rotation, i] + offsetsY[targetRotation, i]);
            }

            bool validPositionFound = false;
            int testIndex = 0;
            while (validPositionFound == false && testIndex < 5)
            {
                bool collisionFound = false;
                int pieceIndex = 0;
                while (collisionFound == false && pieceIndex < 4)
                {
                    VecInt2 pos = pieces[pieceIndex] + testOffsets[testIndex];
                    if (pos.x < 0 || pos.y < 0 || pos.x >= Board.width || pos.y >= Board.height)
                    {
                        collisionFound = true;
                    }
                    else
                    {
                        collisionFound = board.cells[pos.x, pos.y] != 0;
                    }
                    pieceIndex++;
                }

                if (!collisionFound)
                {
                    validPositionFound = true;
                }
                else
                {
                    testIndex++;
                }
            }

            if (validPositionFound)
            {
                piece.rotation = targetRotation;
                piece.positionX += testOffsets[testIndex].x;
                piece.positionY += testOffsets[testIndex].y;
            }

            return validPositionFound;
        }
    }

    class Board
    {
        public const int width = 10;
        public const int height = 22;

        public int[,] cells = new int[width, height];
    }

    class Piece
    {
        public int type;
        public int rotation; //0 - none, 1 - anti clockwise, 2 - 180 deg, 3 - clockwise

        public int positionX;
        public int positionY;

        public Piece(int type, int rotation, int positionX, int positionY)
        {
            this.type = type;
            this.rotation = rotation;
            this.positionX = positionX;
            this.positionY = positionY;
        }
    }

    static class PieceData
    {
        public static readonly int[,] pieceOffsetsX = new int[7, 3]
        {
            { -1, 1, 2 },
            { -1, -1, 1 },
            { 1, -1, 1 },
            { 0, 1, 1 },
            { 0, 1, -1 },
            { 0, -1, 1 },
            { -1, 0, 1 },
        };
        public static readonly int[,] pieceOffsetsY = new int[7, 3]
        {
            { 0, 0, 0 },
            { -1, 0, 0 },
            { -1, 0, 0 },
            { -1, -1, 0 },
            { -1, -1, 0 },
            { -1, 0, 0 },
            { -1, -1, 0 },
        };

        //srs offsets [rotationId, kickId]

        public static readonly int[,] srsOffsetsX_J = new int[4, 5]
        {
            { 0, 0, 0, 0, 0 },
            { 0, 1, 1, 0, 1 },
            { 0, 0, 0, 0, 0 },
            { 0, -1, -1, 0, -1 },
        };
        public static readonly int[,] srsOffsetsY_J = new int[4, 5]
        {
            { 0, 0, 0, 0, 0 },
            { 0, 0, 1, -1, -2 }, // technically should be { 0, 0, 1, -2, -2 } according to the wiki
            { 0, 0, 0, 0, 0 },
            { 0, 0, 1, -1, -2 }, // but that made rotations weird
        };

        public static readonly int[,] srsOffsetsX_I = new int[4, 5]
        {
            { 0, -1, 2, -1, 2 },
            { -1, 0, 0, 0, 0 },
            { -1, 1, -2, 1, -2 },
            { 0, 0, 0, 0, 0 },
        };
        public static readonly int[,] srsOffsetsY_I = new int[4, 5]
        {
            { 0, 0, 0, 0, 0 },
            { 0, 0, 0, -1, 2 },
            { -1, -1, -1, 0, 0 },
            { -1, -1, -1, 1, 2 },
        };

        public static readonly int[,] srsOffsetsX_O = new int[4, 5]
        {
            { 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0 },
            { -1, -1, -1, -1, -1 },
            { -1, -1, -1, -1, -1 },
        };
        public static readonly int[,] srsOffsetsY_O = new int[4, 5]
        {
            { 0, 0, 0, 0, 0 },
            { 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1 },
            { 0, 0, 0, 0, 0 },
        };

        public static VecInt2 GetRotatedPieceOffset(int pieceType, int rotation, int pieceOffsetIndex)
        {
            int x = pieceOffsetIndex == 0 ? 0 : pieceOffsetsX[pieceType, pieceOffsetIndex - 1];
            int y = pieceOffsetIndex == 0 ? 0 : pieceOffsetsY[pieceType, pieceOffsetIndex - 1];

            int srsOffsetX = 0;
            int srsOffsetY = 0;

            switch (pieceType)
            {
                case 0:
                    srsOffsetX = srsOffsetsX_I[rotation, 0];
                    srsOffsetY = srsOffsetsY_I[rotation, 0];
                    break;
                case 3:
                    srsOffsetX = srsOffsetsX_O[rotation, 0];
                    srsOffsetY = srsOffsetsY_O[rotation, 0];
                    break;
                default:
                    srsOffsetX = srsOffsetsX_J[rotation, 0];
                    srsOffsetY = srsOffsetsY_J[rotation, 0];
                    break;
            }


            x += srsOffsetX;
            y += srsOffsetY;

            return rotation switch
            {
                0 => new VecInt2(x, y),
                1 => new VecInt2(y, -x),
                2 => new VecInt2(-x, -y),
                3 => new VecInt2(-y, x),
                _ => throw new Exception($"Invalid rotation id: {rotation}")
            };
        }
    }

    public class VecInt2
    {
        public int x, y;

        public static VecInt2 operator +(VecInt2 a, VecInt2 b)
        {
            return new VecInt2(a.x + b.x, a.y + b.y);
        }
        public static VecInt2 operator -(VecInt2 a, VecInt2 b)
        {
            return new VecInt2(a.x - b.x, a.y - b.y);
        }

        public VecInt2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    static class BoardRenderer
    {
        public static int gridSize = 32;
        const float outlineBrightness = 0.69f;
        const float outlineThickness = 0.08f;

        public static readonly Color backgroundColor = Color.BLACK;
        public static readonly Color gridColor = new Color(35, 35, 35, 255);

        public static readonly Color[] pieceColors =
        {
            new Color(0, 230, 254, 255),
            new Color(24, 1, 255, 255),
            new Color(255, 115, 8, 255),
            new Color(255, 222, 0, 255),
            new Color(102, 253, 0, 255),
            new Color(184, 2, 253, 255),
            new Color(254, 16, 60, 255),
        };

        public static void DrawBoard(Board board, int ox, int oy)
        {
            Raylib.DrawRectangle(ox, oy, gridSize * Board.width, gridSize * Board.height, backgroundColor);
            DrawBoardGrid(ox, oy);
            for (int y = 0; y < Board.height; y++)
            {
                for (int x = 0; x < Board.width; x++)
                {
                    if (board.cells[x, y] != 0)
                    {
                        DrawBoardCell(board, ox, oy, x, y);
                    }
                }
            }
        }

        public static void DrawBoardGrid(int ox, int oy)
        {
            for (int y = 1; y < Board.height; y++)
            {
                Raylib.DrawLine(ox, oy + (gridSize * y), ox + (gridSize * Board.width), oy + (gridSize * y), gridColor);
                Raylib.DrawLine(ox, oy + (gridSize * y) - 1, ox + (gridSize * Board.width), oy + (gridSize * y) - 1, gridColor);
            }
            for (int x = 1; x < Board.width; x++)
            {
                Raylib.DrawLine(ox + (gridSize * x), oy, ox + (gridSize * x), oy + (gridSize * Board.height), gridColor);
                Raylib.DrawLine(ox + (gridSize * x) + 1, oy, ox + (gridSize * x) + 1, oy + (gridSize * Board.height), gridColor);
            }
        }

        public static void DrawBoardCell(Board board, int ox, int oy, int x, int y)
        {
            Color col = pieceColors[board.cells[x, y] - 1];
            Color outlineColor = new Color((int)(col.r * outlineBrightness), (int)(col.g * outlineBrightness), (int)(col.b * outlineBrightness), 255);
            DrawCell(ox + (x * gridSize), oy + (y * gridSize), gridSize, col, outlineColor);
        }

        public static void DrawCell(int x, int y, int size, Color fill, Color outline)
        {
            Raylib.DrawRectangle(x, y, size, size, outline);

            int innerOffset = (int)(gridSize * outlineThickness);
            Raylib.DrawRectangle(x + innerOffset, y + innerOffset, size - (2 * innerOffset), size - (2 * innerOffset), fill);

        }

        public static Color GetOutlineColor(Color col)
        {
            return new Color((int)(col.r * outlineBrightness), (int)(col.g * outlineBrightness), (int)(col.b * outlineBrightness), 255);
        }

        public static void DrawPiece(Piece p, int x, int y, int size)
        {
            Color fill = pieceColors[p.type];
            Color outline = GetOutlineColor(fill);
            for (int i = 0; i < 4; i++)
            {
                VecInt2 offset = PieceData.GetRotatedPieceOffset(p.type, p.rotation, i);
                DrawCell(x + (offset.x * size), y + (offset.y * size), size, fill, outline);
            }
        }

        public static void DrawPieceOnBoard(Piece p, int ox, int oy)
        {
            DrawPiece(p, ox + (p.positionX * gridSize), oy + (p.positionY * gridSize), gridSize);
        }
    }
}
