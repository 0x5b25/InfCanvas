using System;
using System.Collections.Generic;
using System.Text;
using SkiaSharp;

namespace CanvasApp.Types
{
    class TreeDefs{
        public static readonly int ChunkWidth = 400;
        public static readonly int ChunkHeight = 400;

        public static SKColor Pattern_ChessBord(int x,int y)
        {
            if (((x & (1 << 2)) ^ (y & (1 << 2))) == 0){
                return SKColors.DarkGray;
            }
            return SKColors.Gray;
        }
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
        internal ScalablePixelTree tree;
        //internal int depth;
        //int _Xref(int depth) {  return (TreeDefs.ChunkWidth << depth) >> 3; }
        //int _Yref(int depth) { return (TreeDefs.ChunkHeight << depth) >> 3; }
        public ScalablePixelTreeNode(ScalablePixelTreeNode parent, ScalablePixelTree tree) { this.parent = parent; this.tree = tree; }
        internal int xref, yref;
        internal void SetRef(int xref, int yref)
        {
            this.xref = xref;this.yref = yref;
            if (tr == null) return;
            //tr.xref = tl.xref = br.xref = bl.xref = xref >> 1;
            //tr.yref = tl.yref = br.yref = bl.yref = yref >> 1;
            bl.SetRef(xref >> 1, yref >> 1); br.SetRef(xref >> 1, yref >> 1);
            tl.SetRef(xref >> 1, yref >> 1); tr.SetRef(xref >> 1, yref >> 1);
        }

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
            if (n._pixels != null)
                //Only get average value if child node doesnt have children
                return Aver(
                n._GetPixel(tx, ty, 1),
                n._GetPixel(tx + 1, ty, 1),
                n._GetPixel(tx, ty + 1, 1),
                n._GetPixel(tx + 1, ty + 1, 1)
                );
            else return n._GetPixel(tx, ty, 1);

        }
        SKColor _GetPixelFromThis(int x, int y)
        {
            return _pixels[x + TreeDefs.ChunkWidth/2, y + TreeDefs.ChunkHeight/2];
        }
        //Assume coordinates in full depth, depth is used to limit detail level
        internal SKColor _GetPixel(int xglobal, int yglobal, int depth)
        {
            if (depth == 1)
            {
                //Display chunk boundry check

                //if (xglobal >= -TreeDefs.ChunkWidth / 2 && xglobal < TreeDefs.ChunkWidth / 2 &&
                //    yglobal >= -TreeDefs.ChunkHeight / 2 && yglobal < TreeDefs.ChunkHeight / 2)
                {
                    if (-xglobal == TreeDefs.ChunkWidth / 2 || xglobal == TreeDefs.ChunkWidth / 2 - 1 ||
                        -yglobal == TreeDefs.ChunkHeight / 2 || yglobal == TreeDefs.ChunkHeight / 2 - 1)
                    {
                        if (_pixels != null)
                        {
                            return SKColors.Blue;
                        }
                        if (tr != null)
                            return SKColors.Green;
                        return SKColors.Red;
                    }
                    if (_pixels == null)
                    {
                        return _GetPixelFromChild(xglobal, yglobal);
                    }
                    else return _GetPixelFromThis(xglobal, yglobal);
                }
                //else return SKColors.Transparent;

            }
            else if(depth > 1)
            {
                if (tr == null)
                {
                    xglobal >>= depth - 1;
                    yglobal >>= depth - 1;
                    /*while (depth > 1)
                    {
                        xglobal = xglobal == -1 ? -1 : xglobal / 2; yglobal = yglobal == -1 ? -1 : yglobal / 2;
                        depth--;
                    }*/
                    if (_pixels != null)
                    {
                        return _GetPixelFromThis(xglobal, yglobal);
                    }
                    else return SKColors.Gray;
                }

                //int xref = _Xref(depth);
                //int yref = _Yref(depth);

                if (xglobal >= 0)
                {
                    if(yglobal >= 0) return tr._GetPixel(xglobal - xref, yglobal - yref, depth - 1);
                    else return br._GetPixel(xglobal - xref, yglobal + yref, depth - 1);
                }
                else
                {
                    if (yglobal >= 0) return tl._GetPixel(xglobal + xref, yglobal - yref, depth - 1);
                    else return bl._GetPixel(xglobal + xref, yglobal + yref, depth - 1);
                }
            }

            return SKColors.Transparent;
        }
        //Assume coordinates in full depth, depth is used to limit detail level
        internal SKColor _GetPixelNoTransparent(int xglobal, int yglobal, int depth)
        {
            SKColor c = _GetPixel(xglobal, yglobal, depth);
            if (c == SKColors.Transparent || c == 0) return TreeDefs.Pattern_ChessBord(xglobal, yglobal);
            return c;
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
            n._SetPixel(xcoord, ycoord, 1,1, color);
            n._SetPixel(xcoord + 1, ycoord, 1, 1, color);
            n._SetPixel(xcoord, ycoord + 1, 1, 1, color);
            n._SetPixel(xcoord + 1, ycoord + 1, 1, 1, color);
            return;
        }
        void _SetPixelInThis(int x, int y, SKColor color)
        {
            _pixels[x + TreeDefs.ChunkWidth/2, y + TreeDefs.ChunkHeight/2] = color;
        }
        /// <summary>
        /// Set the pixel in specific level of detail
        /// </summary>
        /// <param name="xglobal">global x coord</param>
        /// <param name="yglobal">global y coord</param>
        /// <param name="totalDepth">total depth the tree has</param>
        /// <param name="lod">target lod, make sure lod is nor less than totaldepth</param>
        /// <param name="color"></param>
        internal void _SetPixel(int xglobal, int yglobal, int totalDepth, int lod, SKColor color)
        {
            if (lod == 1)
            {
                xglobal >>= totalDepth - 1;yglobal >>= totalDepth - 1;
                //if (xglobal >= -TreeDefs.ChunkWidth / 2 && xglobal < TreeDefs.ChunkWidth / 2 &&
                //    yglobal >= -TreeDefs.ChunkHeight / 2 && yglobal < TreeDefs.ChunkHeight / 2)
                {
                    if (tl != null)
                    {
                        _SetPixelInChild(xglobal, yglobal, color);
                        return;
                    }
                    if (_pixels == null)
                    {
                        _pixels = new SKColor[TreeDefs.ChunkWidth , TreeDefs.ChunkHeight];
                    }
                    _SetPixelInThis(xglobal, yglobal, color);
                    return;
                }
            }
            else if (lod > 1)
            {
                ScalablePixelTreeNode n;
                int xcoord, ycoord;
                bool t = false, r = false;
                //int scaleFactor = depth - 1;
                //int xref = _Xref(depth);
                //int yref = _Yref(depth);
                if (xglobal >= 0)
                {
                    r = true;
                    xcoord = xglobal - xref;
                    if (yglobal >= 0) { t = true; n = tr; ycoord = yglobal - yref; }
                    else { n = br; ycoord = yglobal + yref; }
                }
                else
                {
                    xcoord = xglobal + xref;
                    if (yglobal >= 0) {  n = tl; ycoord = yglobal - yref; }
                    else { n = bl; ycoord = yglobal + yref; }
                }
                if (n == null)
                {
                    tl = new ScalablePixelTreeNode(this, tree) { xref = this.xref >> 1, yref = this.yref >> 1};
                    tr = new ScalablePixelTreeNode(this,tree){ xref = this.xref >> 1, yref = this.yref >> 1};
                    bl = new ScalablePixelTreeNode(this,tree){ xref = this.xref >> 1, yref = this.yref >> 1};
                    br = new ScalablePixelTreeNode(this,tree) { xref = this.xref >> 1, yref = this.yref >> 1 };
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
                        //tree.totalDepth++;
                    }
                }
                n._SetPixel(xcoord, ycoord, totalDepth - 1,lod - 1, color);
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
        public void SubdivideNoScale()
        {
            if (_pixels == null) return;

            tl = new ScalablePixelTreeNode(this, tree);//{ parent = this, xref = this.xref, yref = this.yref };
            tr = new ScalablePixelTreeNode(this, tree);//{ parent = this, xref = this.xref, yref = this.yref };
            bl = new ScalablePixelTreeNode(this, tree);//{ parent = this, xref = this.xref, yref = this.yref };
            br = new ScalablePixelTreeNode(this, tree);//{ parent = this, xref = this.xref, yref = this.yref };
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
        internal int totalDepth = 0;

        public SKColor GetPixel(int x, int y, int depth)
        {
            ScaleCoord(ref x, ref y, depth);
            if (root == null ||
                x < -root.xref << 1 || x >= root.xref << 1 ||
                y < -root.yref << 1 || y >= root.yref << 1)
            {
                return TreeDefs.Pattern_ChessBord(x, y);
            }
            return root._GetPixelNoTransparent(x, y, totalDepth);
        }

        public int SetPixel(int x, int y, int depth, SKColor color)
        {
            int depthChanged = 0;
            if (root == null)
            {
                root = new ScalablePixelTreeNode(null, this) { xref = TreeDefs.ChunkWidth >> 2, yref = TreeDefs.ChunkHeight >> 2 };
                totalDepth++;
            }
            if (depth <= 0)
            {
                //Exceeding maximum height, need to "push down" viewport
                AddDepth(-depth + 1);
                //So the 'depthChanged' is required to record push down levels
                depthChanged = -depth + 1;
                depth = 1;

            }
            else if(depth > totalDepth)
            {
                //Just bump up totalDepth and reference points
                totalDepth = depth;
                root.SetRef((TreeDefs.ChunkWidth << totalDepth) >>3, (TreeDefs.ChunkHeight << totalDepth) >> 3);
            }

            ScaleCoord(ref x, ref y, depth);

            //evaluate boundary
            //if (x < -root.xref << 1 || x >= root.xref << 1 ||
            //    y < -root.yref << 1 || y >= root.yref << 1) return 0;
            //Check whether depth is adequate
            int w = root.xref * 2;
            int h = root.yref * 2;
            //additional depth needed
            int reqDepth = 0;
            if (x >= w )
            {
                //Log2(x / (root.xref * 2)) - depth + 1
                reqDepth = Utilities.Mathi.Log2(x / root.xref) - 1;
                //out of bounds
            }
            else if(x < -w)
            {
                reqDepth = Utilities.Mathi.Log2(-x / root.xref) - 1;
            }
            if (y >= h)
            {
                int r = Utilities.Mathi.Log2(y / root.xref) - 1;
                reqDepth = r > reqDepth ? r : reqDepth;
            }
            else if (y < -h)
            {
                int r = Utilities.Mathi.Log2(-y / root.xref) - 1;
                reqDepth = r > reqDepth ? r : reqDepth;
            }
            //Slice root and add height
            AddDepth(reqDepth);
            root._SetPixel(x, y, totalDepth,depth + reqDepth, color);

            //Return depth added
            return depthChanged+reqDepth;
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
                        target.tl = new ScalablePixelTreeNode(target, this);
                        target.tr = new ScalablePixelTreeNode(target, this);
                        target.bl = new ScalablePixelTreeNode(target,this);
                        target.br = new ScalablePixelTreeNode(target,this);
                    }
                    else
                    {
                        switch (prePos)
                        {
                            case 0:
                                {
                                    target.tl = pre;
                                    target.tr = new ScalablePixelTreeNode(target, this);
                                    target.bl = new ScalablePixelTreeNode(target,this);
                                    target.br = new ScalablePixelTreeNode(target,this);
                                }
                                break;

                            case 1:
                                {
                                    target.tl = new ScalablePixelTreeNode(target, this);
                                    target.tr = pre;
                                    target.bl = new ScalablePixelTreeNode(target, this);
                                    target.br = new ScalablePixelTreeNode(target,this);
                                }
                                break;

                            case 2:
                                {
                                    target.tl = new ScalablePixelTreeNode(target, this);
                                    target.tr = new ScalablePixelTreeNode(target,this);
                                    target.bl = pre;
                                    target.br = new ScalablePixelTreeNode(target, this);
                                }
                                break;

                            case 3:
                                {
                                    target.tl = new ScalablePixelTreeNode(target, this);
                                    target.tr = new ScalablePixelTreeNode(target,this);
                                    target.bl = new ScalablePixelTreeNode(target,this);
                                    target.br = pre;

                                }
                                break;
                        }
                        pre.parent = target;pre.tree = this;
                    }
                }

                if (root._pixels != null)
                {
                    //need slicing
                    root.SubdivideNoScale();
                    totalDepth++;
                }
                else
                {
                    ScalablePixelTreeNode n = new ScalablePixelTreeNode(null,this);
                    //Calculate ref points based on depth
                    totalDepth++;
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
                    
                }
                root.SetRef((TreeDefs.ChunkWidth << totalDepth) >> 3, (TreeDefs.ChunkHeight << totalDepth) >> 3);
            }
            if (d > 1) AddDepth(d - 1);
        }

        void ScaleCoord(ref int originalX, ref int originalY, int originalDepth)
        {
            if (originalDepth < totalDepth)
            {
                //below max depth, scale up
                originalX <<= (totalDepth - originalDepth);
                originalY <<= (totalDepth - originalDepth);
            }
            if (originalDepth > totalDepth)
            {
                // over max depth, scale down
                originalX >>= (originalDepth - totalDepth);
                originalY >>= (originalDepth - totalDepth);
            }
        }
    }

}
