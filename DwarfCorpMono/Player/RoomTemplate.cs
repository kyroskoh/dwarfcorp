﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace DwarfCorp
{
    [Flags]
    public enum RoomTile
    {
        None   = 0,
        Pillow = 1 << 0,
        Bed    = 1 << 1,
        Table  = 1 << 2,
        Lamp   = 1 << 3,
        Barrel = 1 << 4,
        Wall   = 1 << 5,
        Open   = 1 << 6,
        Edge   = 1 << 7,
        Chair  = 1 << 8,
        Flag   = 1 << 9,
        Anvil  = 1 << 10,
        Forge  = 1 << 11,
        BookTable = 1 << 12,
        PotionTable = 1 << 13,
        Target      = 1 << 14,
        Strawman    = 1 << 15,
        Wheat       = 1 << 16,
        Mushroom    = 1 << 17


    }

    public class RoomTemplate
    {
        public RoomTile[,] Template { get; set; }
        public RoomTile[,] Accessories { get; set; }
        public bool CanRotate { get; set; }

        public void RotateClockwise(int numRotations)
        {
            for (int i = 0; i < numRotations; i++)
            {
                Template = Datastructures.RotateClockwise(Template);
                Accessories = Datastructures.RotateClockwise(Accessories);
            }
        }

        public RoomTemplate(RoomTile[,] template, RoomTile[,] accessories)
        {
            Template = template;
            Accessories = accessories;
            CanRotate = true;
        }

        public RoomTemplate(int sx, int sy)
        {
            Template = new RoomTile[sx, sy];
            Accessories = new RoomTile[sx, sy];

            for (int x = 0; x < sx; x++)
            {
                for (int y = 0; y < sy; y++)
                {
                    Template[x, y] = RoomTile.None;
                    Accessories[x, y] = RoomTile.None;
                }
            }
        }

        public int PlaceTemplate(ref RoomTile[,] room, int seedR, int seedC)
        {
            int nr = room.GetLength(0);
            int nc = room.GetLength(1);
            int tr = Template.GetLength(0);
            int tc = Template.GetLength(1);

            bool conflictFound = false;
            for (int r = 0; r < tr; r++)
            {
                for (int c = 0; c < tc; c++)
                {
                    int x = seedR + r;
                    int y = seedC + c;

                    RoomTile desired = Template[r, c];



                    // Ignore tiles with unspecified conditions
                    if (desired == RoomTile.None)
                    {
                        continue;
                    }

                    if ((x >= nr - 1  || y >= nc - 1 || x <= 0 || y <= 0) && !Has(desired,RoomTile.Edge))
                    {
                        conflictFound = true;
                        return -1;
                    }

                    RoomTile curent = room[x, y];

                    bool hasWall = Has(desired, RoomTile.Wall);
                    bool hasOpen = Has(desired, RoomTile.Open);
                    bool hasEdge = Has(desired, RoomTile.Edge);

      
                    bool meetsWallRequirements = !hasWall || (hasWall && curent == RoomTile.Wall);
                    bool meetsEdgeRequirements = (!hasEdge && !hasWall) || (hasEdge && curent == RoomTile.Edge);
                    bool meetsOpenRequriments = !hasOpen || (hasOpen && curent == RoomTile.Open);
                    bool meetsOtherRequirements = (!hasWall && !hasOpen && !hasEdge) && (curent == RoomTile.Open);

                    // Tiles conflict when walls exist in the room already, or other objects
                    // block the template.
                    if (!(((meetsWallRequirements || meetsEdgeRequirements) && meetsOpenRequriments) || meetsOtherRequirements))
                    {
                        conflictFound = true;
                        return -1;
                    }
                     

                }
            }

            // If we found a conflict, the room could not be placed.
            if (conflictFound)
            {
                return -1;
            }

            int toReturn = 0;
            // Otherwise, we return the number of tiles which could be successfully placed.
            for (int r = 0; r < tr; r++)
            {
                for (int c = 0; c < tc; c++)
                {
                    int x = seedR + r;
                    int y = seedC + c;

                    if (x >= nr - 1 || y >= nc - 1 || x <= 0 || y <= 0)
                    {
                        continue;
                    }

                    RoomTile desiredTile = Template[r, c];
                    RoomTile unimport = Accessories[r, c];
                    RoomTile currentTile = room[x, y];

                    if ((currentTile == RoomTile.Open || currentTile == RoomTile.Edge) && desiredTile != RoomTile.None && !Has(desiredTile, RoomTile.Edge) && ! Has(desiredTile, RoomTile.Wall))
                    {
                        room[x, y] = desiredTile;
                        toReturn++;
                    }

                    if ((currentTile == RoomTile.Open || currentTile == RoomTile.Edge) && unimport != RoomTile.Open && unimport != RoomTile.None && unimport != RoomTile.Edge)
                    {
                        room[x, y] = unimport;
                        toReturn++;
                    }
                }
            }

            return toReturn;
            
        }

        public static bool Has(RoomTile requirements, RoomTile value)
        {
            return (requirements & value) == value;
        }

        public static RoomTile[,] CreateFromRoom(Room room, ChunkManager chunks)
        {
            BoundingBox box0 = room.GetBoundingBox();
            BoundingBox box = new BoundingBox(box0.Min + Vector3.Up, box0.Max + Vector3.Up);
            BoundingBox bigBox = new BoundingBox(box0.Min + Vector3.Up + new Vector3(-1, 0, -1), box0.Max + Vector3.Up + new Vector3(1, 0, 1));
            int nr = (int)(box.Max.X - box.Min.X);
            int nc = (int)(box.Max.Z - box.Min.Z);

            RoomTile[,] toReturn = new RoomTile[nr + 2, nc + 2];

            Dictionary<Point, VoxelRef> voxelDict = new Dictionary<Point, VoxelRef>();
            List<VoxelRef> voxelsInRoom = chunks.GetVoxelsIntersecting(bigBox);
            foreach (VoxelRef vox in voxelsInRoom)
            {
                voxelDict[new Point((int)(vox.WorldPosition.X - box.Min.X) + 1, (int)(vox.WorldPosition.Z - box.Min.Z) + 1)] = vox;
            }

            for (int r = 0; r < nr + 2; r++)
            {
                for (int c = 0; c < nc + 2; c++)
                {
                    toReturn[r, c] = RoomTile.Edge;
                }
            }

            foreach (KeyValuePair<Point, VoxelRef> voxPair in voxelDict)
            {
                VoxelRef vox = voxPair.Value;
                Point p = voxPair.Key;

                if (vox.TypeName == "empty" && p.X > 0 && p.X < nr + 1 && p.Y > 0 && p.Y < nc + 1)
                {
                    toReturn[p.X, p.Y] = RoomTile.Open;
                }
                else if (vox.TypeName != "empty") 
                {
                    toReturn[p.X, p.Y] = RoomTile.Wall;
                }
            }

            return toReturn;
        }
    }
}
