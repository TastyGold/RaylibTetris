using Raylib_cs;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;

namespace RaylibTetris
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            Raylib.InitWindow(1600, 1200, "Raylibtris");
            Raylib.SetTargetFPS(4);

            Board board = new Board();
            Random rand = new Random();

            Piece boobs = new Piece(0, 0);

            for (int y = 0; y < Board.height; y++)
            {
                for (int x = 0; x < Board.width; x++)
                {
                    //board.cells[x, y] = rand.Next(0, 8);
                }
            }

            while (!Raylib.WindowShouldClose())
            {
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.DARKGRAY);
                BoardRenderer.DrawBoard(board, 50, 50);
                BoardRenderer.DrawPiece(boobs, 150, 150, BoardRenderer.gridSize);
                boobs.rotation++;
                boobs.rotation %= 4;
                if (boobs.rotation == 0)
                {
                    boobs.type++;
                    boobs.type %= 7;
                }

                Raylib.EndDrawing();
            }

            Raylib.CloseWindow();
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
        public int rotation; //0 - none, 1 - clockwise, 2 - 180 deg, 3 - anti clockwise

        public Piece(int type, int rotation)
        {
            this.type = type;
            this.rotation = rotation;
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
            { 0, 0, 1, -2, -2 },
            { 0, 0, 0, 0, 0 },
            { 0, 0, 1, -2, -2 },
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
            { -1, -1, -1, -1, -1 },
            { -1, -1, -1, -1, -1 },
            { 0, 0, 0, 0, 0 },
        };

        public static VecInt2 GetRotatedPieceOffset(int pieceType, int rotation, int pieceOffsetIndex)
        {
            int x = pieceOffsetsX[pieceType, pieceOffsetIndex];
            int y = pieceOffsetsY[pieceType, pieceOffsetIndex];

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

        public VecInt2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    static class BoardRenderer
    {
        public static int gridSize = 30;
        const float outlineBrightness = 0.55f;
        const float outlineThickness = 0.08f;

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
            Raylib.DrawRectangle(ox, oy, gridSize * Board.width, gridSize * Board.height, Color.BLACK);
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
            DrawCell(x, y, size, fill, outline);
            for (int i = 0; i < 3; i++)
            {
                VecInt2 offset = PieceData.GetRotatedPieceOffset(p.type, p.rotation, i);
                DrawCell(x + (offset.x * size), y + (offset.y * size), size, fill, outline);
            }
        }
    }
}