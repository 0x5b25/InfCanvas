using System;
using System.Collections.Generic;
using System.Text;
using SkiaSharp;

namespace CanvasApp.Types
{
    class TreeDefs{
        public static readonly int ChunkWidth = 400;
        public static readonly int ChunkHeight = 400;
    }

    /*     
     *  bl | br
     * ----0---- x+
     *  tl | tr
     *     y+
     * -------------   
     * | 0,0 | w,0 |
     * |---- 0---- x+
     * | 0,h | w,h |
     * ---- y+ -----
     */

    class ScalablePixelTreeNode
    {
        internal SKColor[,] _pixels /*= new SKColor[TreeDefs.ChunkWidth*TreeDefs.ChunkHeight]*/;
        internal ScalablePixelTreeNode tl, tr, bl, br;
        internal ScalablePixelTreeNode parent;
        //internal int depth;
        int _Xref(int depth) {  return (TreeDefs.ChunkWidth << depth) >> 3; }
        int _Yref(int depth) { return (TreeDefs.ChunkHeight << depth) >> 3; }

        SKColor _GetPixelFromChild(int x, int y)
        {
            if (tr == null) return SKColors.Transparent;

            //Scale
            /*             -------------------
             *  -------    | chunk3 | chunk2 |
             *  | SRC | -> |--------|--------|
             *  -------    | chunk1 | chunk0 |
             *             -------------------
             */
            SKColor Aver(SKColor p0, SKColor p1, SKColor p2, SKColor p3)
            {
                return new SKColor(
                    (byte)((p0.Red + p1.Red + p2.Red + p3.Red) >> 2),
                    (byte)((p0.Green + p1.Green + p2.Green + p3.Green) >> 2),
                    (byte)((p0.Blue + p1.Blue + p2.Blue + p3.Blue) >> 2),
                    (byte)((p0.Alpha + p1.Alpha + p2.Alpha + p3.Alpha) >> 2)
                    );
            }

            //Find out sector
            int xcenter = TreeDefs.ChunkWidth / 2;
            int ycenter = TreeDefs.ChunkHeight / 2;
            int tx, ty;
            ScalablePixelTreeNode n;
            int index = y * TreeDefs.ChunkWidth;
            if (y >= 0)
            {
                ty = y * 2 - TreeDefs.ChunkHeight / 2;
                //Chunk 0
                if (x >= 0) { tx = x * 2 - TreeDefs.ChunkWidth / 2; n = tr; }
                //Chunk 1
                else { tx = x * 2 + TreeDefs.ChunkWidth / 2; n = tl; }
            }
            else
            {
                ty = y * 2 + TreeDefs.ChunkHeight / 2;
                //Chunk 0
                if (x >= 0) { tx = x * 2 - TreeDefs.ChunkWidth / 2; n = br; }
                //Chunk 3
                else { tx = x * 2 + TreeDefs.ChunkWidth / 2; n = bl; }
            }
            return Aver(
                n._GetPixel(tx, ty, 1),
                n._GetPixel(tx + 1, ty, 1),
                n._GetPixel(tx, ty + 1, 1),
                n._GetPixel(tx + 1, ty + 1, 1)
                );

        }
        SKColor _GetPixelFromThis(int x, int y)
        {
            return _pixels[x + TreeDefs.ChunkWidth/2, y + TreeDefs.ChunkHeight/2];
        }
        internal SKColor _GetPixel(int x, int y, int depth)
        {
            if (depth == 1)
            {
                //Display chunk boundry check

                if (x >= -TreeDefs.ChunkWidth / 2 && x < TreeDefs.ChunkWidth / 2 &&
                    y >= -TreeDefs.ChunkHeight / 2 && y < TreeDefs.ChunkHeight / 2)
                {
                    if (-x == TreeDefs.ChunkWidth / 2 || x == TreeDefs.ChunkWidth / 2 - 1 ||
                        -y == TreeDefs.ChunkHeight / 2 || y == TreeDefs.ChunkHeight / 2 - 1)
                    {
                        if (_pixels != null)
                        {
                            return SKColors.Blue;
                        }
                        if (tr != null)
                            return SKColors.Green;
                        return SKColors.Red;
                    }
                    if (_pixels == null) return _GetPixelFromChild(x, y);
                    else return _GetPixelFromThis(x,y);
                }
                else return SKColors.Transparent;

            }
            else if(depth > 1)
            {
                if (tr == null)
                {
                    while (depth > 1)
                    {
                        x = x == -1 ? -1 : x / 2; y = y == -1 ? -1 : y / 2;
                        depth--;
                    }
                    if (_pixels != null)
                    {
                        return _GetPixelFromThis(x, y);
                    }
                    else return SKColors.Gray;
                }

                int xref = _Xref(depth);
                int yref = _Yref(depth);

                if (x >= 0)
                {
                    if(y >= 0) return tr._GetPixel(x - xref, y - yref, depth - 1);
                    else return br._GetPixel(x - xref, y + yref, depth - 1);
                }
                else
                {
                    if (y >= 0) return tl._GetPixel(x + xref, y - yref, depth - 1);
                    else return bl._GetPixel(x + xref, y + yref, depth - 1);
                }
            }

            return SKColors.Transparent;
            
        }

        void _SetPixelInChild(int x, int y, SKColor color)
        {
            //Find out which sector to draw on
            ScalablePixelTreeNode n;
            int xcoord, ycoord;
            if (x >= 0)
            {
                xcoord = x * 2 - TreeDefs.ChunkWidth / 2;
                if (y >= 0) { n = tr; ycoord = y * 2 - TreeDefs.ChunkHeight / 2; }
                else { n = br; ycoord = y * 2 + TreeDefs.ChunkHeight / 2; }
            }
            else
            {
                xcoord = x * 2 + TreeDefs.ChunkWidth / 2;
                if (y >= 0) { n = tl; ycoord = y * 2 - TreeDefs.ChunkHeight / 2; }
                else { n = bl; ycoord = y * 2 + TreeDefs.ChunkHeight / 2; }
            }
            n._SetPixel(xcoord, ycoord, 1, color);
            n._SetPixel(xcoord + 1, ycoord, 1, color);
            n._SetPixel(xcoord, ycoord + 1, 1, color);
            n._SetPixel(xcoord + 1, ycoord + 1, 1, color);
            return;
        }
        void _SetPixelInThis(int x, int y, SKColor color)
        {
            _pixels[x + TreeDefs.ChunkWidth/2, y + TreeDefs.ChunkHeight/2] = color;
        }
        internal void _SetPixel(int x, int y, int depth, SKColor color)
        {
            if (depth == 1)
            {
                if (x >= -TreeDefs.ChunkWidth / 2 && x < TreeDefs.ChunkWidth / 2 &&
                    y >= -TreeDefs.ChunkHeight / 2 && y < TreeDefs.ChunkHeight / 2)
                {
                    if (tl != null)
                    {
                        _SetPixelInChild(x, y, color);
                        return;
                    }
                    if (_pixels == null)
                    {
                        _pixels = new SKColor[TreeDefs.ChunkWidth , TreeDefs.ChunkHeight];
                    }
                    _SetPixelInThis(x, y, color);
                    return;
                }
            }
            else if (depth > 1)
            {
                ScalablePixelTreeNode n;
                int xcoord, ycoord;
                bool t = false, r = false;
                //int scaleFactor = depth - 1;
                int xref = _Xref(depth);
                int yref = _Yref(depth);
                if (x >= 0)
                {
                    r = true;
                    xcoord = x - xref;
                    if (y >= 0) { t = true; n = tr; ycoord = y - yref; }
                    else { n = br; ycoord = y + yref; }
                }
                else
                {
                    xcoord = x + xref;
                    if (y >= 0) {  n = tl; ycoord = y - yref; }
                    else { n = bl; ycoord = y + yref; }
                }
                if (n == null)
                {
                    tl = new ScalablePixelTreeNode() { parent = this };
                    tr = new ScalablePixelTreeNode() { parent = this };
                    bl = new ScalablePixelTreeNode() { parent = this };
                    br = new ScalablePixelTreeNode() { parent = this };
                    if (t) { if (r) n = tr; else n = tl; }
                    else { if (r) n = br; else n = bl; }
                    if (_pixels != null)
                    {
                        //Create new chunks
                        SKColor[][,] dst = new SKColor[4][,];
                        dst[0] = new SKColor[TreeDefs.ChunkWidth , TreeDefs.ChunkHeight];
                        dst[1] = new SKColor[TreeDefs.ChunkWidth , TreeDefs.ChunkHeight];
                        dst[2] = new SKColor[TreeDefs.ChunkWidth , TreeDefs.ChunkHeight];
                        dst[3] = new SKColor[TreeDefs.ChunkWidth , TreeDefs.ChunkHeight];

                        //Slice this node
                        Utilities.ParallelJobManager.Get().DoJob((int index, int num) => { SliceAndScaleUp(index, num, dst); });
                        tr._pixels = dst[0]; tl._pixels = dst[1];
                        br._pixels = dst[2]; bl._pixels = dst[3];
                        _pixels = null;
                    }
                }
                n._SetPixel(xcoord, ycoord, depth - 1, color);
            }
        }

        int _pixelIndex(int x, int y) { return (y+TreeDefs.ChunkHeight/2) * TreeDefs.ChunkWidth + x + TreeDefs.ChunkWidth / 2; }

        //Slice chunk into 4 pieces and scale up 2x
        void SliceAndScaleUp(int index, int num, SKColor[][,] dst)
        {
            /*             -------------------
             *  -------    | chunk3 | chunk2 |
             *  | SRC | -> |--------|--------|
             *  -------    | chunk1 | chunk0 |
             *             -------------------
             */

            int xcenter = TreeDefs.ChunkWidth / 2;
            int ycenter = TreeDefs.ChunkHeight / 2;

            for (int y = index; y < TreeDefs.ChunkHeight; y += num)
            {
                if (y >= ycenter)
                {
                    //Chunk 1
                    int y1 = (y - ycenter) *  2;
                    int y2 = y1 + 1;
                    int x = 0;
                    for (int x1 = 0; x < xcenter; x++,x1+=2)
                    {
                        dst[1][x1   , y1] = dst[1][x1   , y2] =
                        dst[1][x1+1 , y1] = dst[1][x1+1 , y2] = _pixels[x,y];
                    }
                    //Chunk 0
                    for (int x1 = 0; x < TreeDefs.ChunkWidth; x++,x1+=2)
                    {
                        dst[0][x1, y1] = dst[0][x1, y2] =
                        dst[0][x1 + 1, y1] = dst[0][x1 + 1, y2] = _pixels[x, y];
                    }
                }
                else
                {
                    //Chunk 3
                    int y1 = y  * 2;
                    int y2 = y1 + 1;
                    int x = 0;
                    for (int x1 = 0; x < xcenter; x++, x1 += 2)
                    {
                        dst[3][x1, y1] = dst[3][x1, y2] =
                        dst[3][x1 + 1, y1] = dst[3][x1 + 1, y2] = _pixels[x, y];
                    }
                    //Chunk 2
                    for (int x1 = 0; x < TreeDefs.ChunkWidth; x++, x1 += 2)
                    {
                        dst[2][x1, y1] = dst[2][x1, y2] =
                        dst[2][x1 + 1, y1] = dst[2][x1 + 1, y2] = _pixels[x, y];
                    }
                }
            }
        }
        void SliceNoScale(int index, int num, SKColor[][,] dst)

        {
            /*
             *             ---------------------------
             *             |  chunk3    |    chunk2  |
             *             |   ---------|---------   |
             *  -------    |   |  src   |  src   |   |
             *  | SRC | -> |---|--------|--------|---|
             *  -------    |   |  src   |  src   |   |
             *             |   ---------|---------   |
             *             |  chunk1    |    chunk0  |
             *             ---------------------------
             */

            int xcenter = TreeDefs.ChunkWidth / 2;
            int ycenter = TreeDefs.ChunkHeight / 2;

            for (int y = index; y < TreeDefs.ChunkHeight; y += num)
            {
                if (y >= ycenter)
                {
                    int y1 = y - ycenter;
                    int x = 0;
                    //Chunk 1
                    for (; x < xcenter; x++)
                    {
                        int x1 = x + xcenter;
                        
                        dst[1][x1, y1] = _pixels[x, y];
                    }
                    //Chunk 0
                    for (; x < TreeDefs.ChunkWidth; x++)
                    {
                        int x1 = x - xcenter;
                        dst[0][x1, y1] = _pixels[x, y];
                    }
                }
                else
                {
                    //Chunk 3
                    int y1 = y + ycenter;
                    int x = 0;
                    for (; x < xcenter; x++)
                    {
                        int x1 = x + xcenter;
                        dst[3][x1, y1] = _pixels[x, y];
                    }
                    //Chunk 2
                    for (; x < TreeDefs.ChunkWidth; x++)
                    {
                        int x1 = x - xcenter;
                        dst[2][x1, y1]  = _pixels[x, y];
                    }
                }
            }
        }
        public void Subdivide()
        {
            if (_pixels == null) return;

            tl = new ScalablePixelTreeNode() { parent = this };
            tr = new ScalablePixelTreeNode() { parent = this };
            bl = new ScalablePixelTreeNode() { parent = this };
            br = new ScalablePixelTreeNode() { parent = this };

            //Create new chunks
            SKColor[][,] dst = new SKColor[4][,];
            dst[0] = new SKColor[TreeDefs.ChunkWidth, TreeDefs.ChunkHeight];
            dst[1] = new SKColor[TreeDefs.ChunkWidth, TreeDefs.ChunkHeight];
            dst[2] = new SKColor[TreeDefs.ChunkWidth, TreeDefs.ChunkHeight];
            dst[3] = new SKColor[TreeDefs.ChunkWidth, TreeDefs.ChunkHeight];

            //Slice this node
            Utilities.ParallelJobManager.Get().DoJob((int index, int num) => { SliceNoScale(index, num, dst); });
            tr._pixels = dst[0]; tl._pixels = dst[1];
            br._pixels = dst[2]; bl._pixels = dst[3];
            _pixels = null;

        }
    }

    class ScalablePixelTree
    {
        ScalablePixelTreeNode root;
        int totalDepth = 0;

        public SKColor GetPixel(int x, int y, int depth)
        {
            if (root != null) return root._GetPixel(x, y, depth);
            return SKColors.Transparent;
        }

        public int SetPixel(int x, int y, int depth, SKColor color)
        {
            if (root == null)
            {
                root = new ScalablePixelTreeNode();
                totalDepth++;
            }
            if (depth <= 0)
            {
                AddDepth(-depth + 1);
                depth = 1;
            }
            //evaluate boundary
            int w = depth >= 2 ? TreeDefs.ChunkWidth << (depth - 2) : TreeDefs.ChunkWidth >> 1;
            int h = depth >= 2 ? TreeDefs.ChunkHeight << (depth - 2) : TreeDefs.ChunkHeight >> 1;
            //additional depth needed
            int reqDepth = 0;
            if (x >= w )
            {
                reqDepth = Utilities.Mathi.Log2(x *2 / TreeDefs.ChunkWidth) - depth + 1;
                //out of bounds
            }
            else if(x < -w)
            {
                reqDepth = Utilities.Mathi.Log2(-x * 2 / TreeDefs.ChunkWidth  ) - depth + 1;
            }
            if (y > h)
            {
                int r = Utilities.Mathi.Log2(y * 2 / TreeDefs.ChunkHeight  ) - depth + 1;
                reqDepth = r > reqDepth ? r : reqDepth;
            }
            else if (y < -h)
            {
                int r = Utilities.Mathi.Log2(-y * 2 / TreeDefs.ChunkHeight  ) - depth + 1;
                reqDepth = r > reqDepth ? r : reqDepth;
            }
            //Slice root and add height
            AddDepth(reqDepth);
            root._SetPixel(x, y, depth + reqDepth, color);

            //Return depth added
            return reqDepth;
        }

        void AddDepth(int d)
        {
            if (d <= 0) return;
            
            //else
            {
                void Assign(ScalablePixelTreeNode target, ScalablePixelTreeNode pre, int prePos)
                {
                    if (pre == null)
                    {
                        target.tl = new ScalablePixelTreeNode() { parent = target };
                        target.tr = new ScalablePixelTreeNode() { parent = target };
                        target.bl = new ScalablePixelTreeNode() { parent = target };
                        target.br = new ScalablePixelTreeNode() { parent = target };
                    }
                    else
                    {
                        switch (prePos)
                        {
                            case 0:
                                {
                                    target.tl = pre;
                                    target.tr = new ScalablePixelTreeNode() { parent = target };
                                    target.bl = new ScalablePixelTreeNode() { parent = target };
                                    target.br = new ScalablePixelTreeNode() { parent = target };
                                }
                                break;

                            case 1:
                                {
                                    target.tl = new ScalablePixelTreeNode() { parent = target };
                                    target.tr = pre;
                                    target.bl = new ScalablePixelTreeNode() { parent = target };
                                    target.br = new ScalablePixelTreeNode() { parent = target };
                                }
                                break;

                            case 2:
                                {
                                    target.tl = new ScalablePixelTreeNode() { parent = target };
                                    target.tr = new ScalablePixelTreeNode() { parent = target };
                                    target.bl = pre;
                                    target.br = new ScalablePixelTreeNode() { parent = target };
                                }
                                break;

                            case 3:
                                {
                                    target.tl = new ScalablePixelTreeNode() { parent = target };
                                    target.tr = new ScalablePixelTreeNode() { parent = target };
                                    target.bl = new ScalablePixelTreeNode() { parent = target };
                                    target.br = pre;

                                }
                                break;
                        }
                        pre.parent = target;
                    }
                }

                if (root._pixels != null)
                {
                    //need slicing
                    root.Subdivide();
                    totalDepth++;
                }
                else
                {
                    ScalablePixelTreeNode n = new ScalablePixelTreeNode();
                    Assign(n, null, 0);
                    if (root.tr != null)
                    {
                        //There is depth
                        //n.tl.br = root.tl;
                        Assign(n.tl, root.tl, 3);
                        //n.tr.bl = root.tr;
                        Assign(n.tr, root.tr, 2);
                        //n.bl.tr = root.bl;
                        Assign(n.bl, root.bl, 1);
                        //n.br.tl = root.br;
                        Assign(n.br, root.br, 0);
                    }
                    root = n;
                    totalDepth++;
                }
            }
            if (d > 1) AddDepth(d - 1);
        }
    }

}
