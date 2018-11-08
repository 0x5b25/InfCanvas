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

    /*     y+
     *  tl | tr
     * ----0---- x+
     *  bl | br
     */

    class ScalablePixelTreeNode
    {
        internal SKColor[] _pixels /*= new SKColor[TreeDefs.ChunkWidth*TreeDefs.ChunkHeight]*/;
        internal ScalablePixelTreeNode tl, tr, bl, br;
        internal ScalablePixelTreeNode parent;
        //internal int depth;
        internal SKColor _getPixel(int x, int y, int depth)
        {
            if(depth == 1)
            {
                int i = _pixelIndex(x, y);
                if (i >= 0 && i < _pixels.Length)
                    return _pixels[i];
                return SKColors.Transparent;
            }else if(depth > 1)
            {
                SKColor get()
                {
                    while (depth > 1)
                    {
                        x = x == -1 ? -1 : x / 2; y = y == -1 ? -1 : y / 2;
                        depth--;
                    }
                    int i = _pixelIndex(x, y);
                    if (i >= 0 && i < _pixels.Length)
                        return _pixels[i];
                    return SKColors.Transparent;
                }
                if(x >= 0)
                {
                    if(y >= 0)
                    {
                        //tr
                        if (tr == null) return get();
                        return tr._getPixel(x - TreeDefs.ChunkWidth / 2, y - TreeDefs.ChunkHeight / 2, depth);
                    }
                    else
                    {
                        //br
                        if (br == null) return get();
                        return br._getPixel(x - TreeDefs.ChunkWidth / 2, y + TreeDefs.ChunkHeight / 2, depth);
                    }
                }
                else
                {
                    if (y >= 0)
                    {
                        //tl
                        if (tl == null) return get();
                        return tl._getPixel(x + TreeDefs.ChunkWidth / 2, y - TreeDefs.ChunkHeight / 2, depth);
                    }
                    else
                    {
                        //bl
                        if (bl == null) return get();
                        return bl._getPixel(x + TreeDefs.ChunkWidth / 2, y + TreeDefs.ChunkHeight / 2, depth);
                    }
                }
            }
            
            return SKColors.Transparent;
            
        }
        internal void _setPixel(int x, int y, int depth, SKColor color)
        {
            if (depth == 1)
            {
                if(_pixels == null)
                {
                    if (tl != null)
                    {
                        //There is a deeper layer
                        //Find out which sector to draw on
                        ScalablePixelTreeNode n;
                        int xcoord, ycoord;
                        if (x >= 0)
                        {
                            xcoord = x*2 - TreeDefs.ChunkWidth / 2;
                            if (y >= 0) { n = tr; ycoord = y*2 - TreeDefs.ChunkHeight/2; }
                            else {  n = br; ycoord = y*2 + TreeDefs.ChunkHeight / 2; }
                        }
                        else
                        {
                            xcoord = x*2 + TreeDefs.ChunkWidth / 2;
                            if (y >= 0) {  n = tl; ycoord = y *2 - TreeDefs.ChunkHeight / 2; }
                            else { n = bl; ycoord = y*2 + TreeDefs.ChunkHeight / 2; }
                        }
                        n._setPixel(xcoord, ycoord, 1, color);
                        n._setPixel(xcoord + 1, ycoord, 1, color);
                        n._setPixel(xcoord, ycoord + 1, 1, color);
                        n._setPixel(xcoord + 1, ycoord + 1, 1, color);
                        return;
                    }
                    else _pixels = new SKColor[TreeDefs.ChunkWidth * TreeDefs.ChunkHeight];
                }
                int i = _pixelIndex(x, y);
                if (i >= 0 && i < _pixels.Length)
                    _pixels[i] = color;
                return;
            }
            else if (depth > 1)
            {
                ScalablePixelTreeNode n;
                int xcoord, ycoord;
                bool t, r;
                if (x >= 0)
                {
                    r = true;
                    xcoord = x * 2 - TreeDefs.ChunkWidth / 2;
                    if (y >= 0) { t = true;/*tr*/ n = tr; ycoord = y *2- TreeDefs.ChunkHeight / 2; }
                    else { t = false; n = br; ycoord = y*2 + TreeDefs.ChunkHeight / 2; }
                }
                else
                {
                    r = false;
                    xcoord = x*2 + TreeDefs.ChunkWidth / 2;
                    if (y >= 0) { t = true; n = tl; ycoord = y *2 - TreeDefs.ChunkHeight / 2; }
                    else { t = false; n = bl; ycoord = y*2 + TreeDefs.ChunkHeight / 2; }
                }
                if (n == null)
                {
                    tl = new ScalablePixelTreeNode() { parent = this };
                    tr = new ScalablePixelTreeNode() { parent = this };
                    bl = new ScalablePixelTreeNode() { parent = this };
                    br = new ScalablePixelTreeNode() { parent = this };
                    if (_pixels != null)
                    {
                        //Create new chunks
                        SKColor[][] dst = new SKColor[4][];
                        dst[0] = new SKColor[TreeDefs.ChunkWidth * TreeDefs.ChunkHeight];
                        dst[1] = new SKColor[TreeDefs.ChunkWidth * TreeDefs.ChunkHeight];
                        dst[2] = new SKColor[TreeDefs.ChunkWidth * TreeDefs.ChunkHeight];
                        dst[3] = new SKColor[TreeDefs.ChunkWidth * TreeDefs.ChunkHeight];

                        //Slice this node
                        Utilities.ParallelJobManager.Get().DoJob((int index, int num) => { Slice(index, num, dst); });
                        tl._pixels = dst[0]; tr._pixels = dst[1];
                        bl._pixels = dst[2]; br._pixels = dst[3];
                        _pixels = null;
                    }

                }
                n._setPixel(xcoord, ycoord, depth - 1, color);
            }
        }
        int _pixelIndex(int x, int y) { return (y+TreeDefs.ChunkHeight/2) * TreeDefs.ChunkWidth + x + TreeDefs.ChunkWidth / 2; }

        //Slice chunk into 4 pieces and scale up 2x
        void Slice(int index, int num, SKColor[][] dst)
        {
            /*             -------------------
             *  -------    | chunk0 | chunk1 |
             *  | SRC | -> |--------|--------|
             *  -------    | chunk2 | chunk3 |
             *             -------------------
             */

            int xcenter = TreeDefs.ChunkWidth / 2;
            int ycenter = TreeDefs.ChunkHeight / 2;

            for (int y = index; y < TreeDefs.ChunkHeight; y += num)
            {
                int x = y * TreeDefs.ChunkWidth;
                if (y >= ycenter)
                {
                    //Chunk 2

                    for (int t1 = (y - ycenter) * TreeDefs.ChunkWidth * 2,
                        t2 = t1 + TreeDefs.ChunkWidth; x < xcenter; x++, t1 += 2, t2 += 2)
                    {
                        dst[2][t1] = dst[2][t1 + 1] =
                        dst[2][t2] = dst[2][t2 + 1] = _pixels[x];
                    }
                    //Chunk 3
                    for (int t1 = (y - ycenter) * TreeDefs.ChunkWidth * 2,
                        t2 = t1 + TreeDefs.ChunkWidth; x < TreeDefs.ChunkWidth; x++, t1 += 2, t2 += 2)
                    {
                        dst[3][t1] = dst[3][t1 + 1] =
                        dst[3][t2] = dst[3][t2 + 1] = _pixels[x];
                    }
                }
                else
                {
                    //Chunk 0
                    for (int t1 = y * TreeDefs.ChunkWidth * 2,
                        t2 = t1 + TreeDefs.ChunkWidth; x < xcenter; x++, t1 += 2, t2 += 2)
                    {
                        dst[0][t1] = dst[0][t1 + 1] =
                        dst[0][t2] = dst[0][t2 + 1] = _pixels[x];
                    }
                    //Chunk 1
                    for (int t1 = y * TreeDefs.ChunkWidth * 2,
                        t2 = t1 + TreeDefs.ChunkWidth; x < TreeDefs.ChunkWidth; x++, t1 += 2, t2 += 2)
                    {
                        dst[1][t1] = dst[1][t1 + 1] =
                        dst[1][t2] = dst[1][t2 + 1] = _pixels[x];
                    }
                }
            }
        }
    }

    class ScalablePixelTree
    {
        ScalablePixelTreeNode root;
        int totalDepth = 0;

        public SKColor GetPixel(int x, int y, int depth)
        {
            if (root != null) return root._getPixel(x, y, depth);
            return SKColors.Transparent;
        }

        public int SetPixel(int x, int y, int depth, SKColor color)
        {
            if (depth <= 0)
            {
                AddDepth(-depth + 1);
                depth = 1;
            }
            //evaluate boundary
            int w = depth * TreeDefs.ChunkWidth / 2;
            int h = depth * TreeDefs.ChunkHeight / 2;
            //additional depth needed
            int reqDepth = 0;
            if (x >= w )
            {
                reqDepth = x *2 / TreeDefs.ChunkWidth;
                //out of bounds
            }
            else if(x < -w)
            {
                reqDepth = -x * 2 / TreeDefs.ChunkWidth;
            }
            if (y > h)
            {
                int r = y * 2 / TreeDefs.ChunkHeight;
                reqDepth = r > reqDepth ? r : reqDepth;
            }
            else if (y < -h)
            {
                int r = -y * 2 / TreeDefs.ChunkHeight;
                reqDepth = r > reqDepth ? r : reqDepth;
            }
            //Slice root and add height
            AddDepth(reqDepth);
            root._setPixel(x, y, depth + reqDepth, color);

            //Return depth added
            return reqDepth;
        }

        void AddDepth(int d)
        {
            if (d <= 0) return;
            if (root == null)
            {
                root = new ScalablePixelTreeNode();
                totalDepth++;
            }
            else
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
                ScalablePixelTreeNode n = new ScalablePixelTreeNode();
                Assign(n, null, 0);
                if (root.tl == null)
                {
                    //No depth
                    if (root._pixels != null)
                    {
                        //need slicing

                        //Create new chunks
                        SKColor[][] dst = new SKColor[4][];
                        dst[0] = new SKColor[TreeDefs.ChunkWidth * TreeDefs.ChunkHeight];
                        dst[1] = new SKColor[TreeDefs.ChunkWidth * TreeDefs.ChunkHeight];
                        dst[2] = new SKColor[TreeDefs.ChunkWidth * TreeDefs.ChunkHeight];
                        dst[3] = new SKColor[TreeDefs.ChunkWidth * TreeDefs.ChunkHeight];

                        //Slice this node
                        Utilities.ParallelJobManager.Get().DoJob((int index, int num) => { Slice(index, num, dst); });
                        n.tl._pixels = dst[0]; n.tr._pixels = dst[1];
                        n.bl._pixels = dst[2]; n.br._pixels = dst[3];
                    }
                }
                else
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
            if (d > 1) AddDepth(d - 1);
        }

        //Slice chunk into 4 pieces without scaling up
        void Slice(int index, int num, SKColor[][] dst)

        {
            /*
             *             ---------------------------
             *             |  chunk0    |    chunk1  |
             *             |   ---------|---------   |
             *  -------    |   |  src   |  src   |   |
             *  | SRC | -> |---|--------|--------|---|
             *  -------    |   |  src   |  src   |   |
             *             |   ---------|---------   |
             *             |  chunk2    |    chunk3  |
             *             ---------------------------
             */

            int xcenter = TreeDefs.ChunkWidth / 2;
            int ycenter = TreeDefs.ChunkHeight / 2;

            for (int y = index; y < TreeDefs.ChunkHeight; y += num)
            {
                int x = y * TreeDefs.ChunkWidth;
                if (y >= ycenter)
                {
                    //Chunk 2

                    for (int t = (y - ycenter) * TreeDefs.ChunkWidth + xcenter; x < xcenter; x++, t ++)
                    {
                        dst[2][t] = root._pixels[x];
                    }
                    //Chunk 3
                    for (int t = (y - ycenter) * TreeDefs.ChunkWidth ; x < TreeDefs.ChunkWidth; x++, t++)
                    {
                        dst[3][t] = root._pixels[x];
                    }
                }
                else
                {
                    //Chunk 0
                    for (int t = (y+ycenter) * TreeDefs.ChunkWidth + xcenter; x < xcenter; x++, t ++)
                    {
                        dst[0][t] = root._pixels[x];
                    }
                    //Chunk 1
                    for (int t = (y + ycenter) * TreeDefs.ChunkWidth; x < TreeDefs.ChunkWidth; x++, t ++)
                    {
                        dst[1][t] = root._pixels[x];
                    }
                }
            }
        }
    }

}
