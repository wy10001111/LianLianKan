using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LianLianKan.Control
{
    /// <summary>
    /// LianLianKanMap.xaml 的交互逻辑
    /// </summary>
    public partial class LianLianKanMap : UserControl
    {
        public LianLianKanMap()
        {
            InitializeComponent();
            NumOfGridDefaultChildren = this.mapGrid.Children.Count;
            this.InitMap(8, 8);
        }

        protected class PathDirection
        {
            public PathDirection(int dRow, int dCol)
            {
                this.DRow = dRow;
                this.DCol = dCol;
            }
            public int DRow { get; set; }
            public int DCol { get; set; }
            public static bool operator ==(PathDirection d1, PathDirection d2)
            {
                if (Object.ReferenceEquals(d1, d2))
                    return true;
                if (Object.ReferenceEquals(d1, null) || Object.ReferenceEquals(d2, null))
                    return false;
                if (d1.DRow == 0 && d1.DCol == 0)
                    return true;
                if (d2.DRow == 0 && d2.DCol == 0)
                    return true;
                return d1.DRow == d2.DRow && d1.DCol == d2.DCol;
            }
            public static bool operator !=(PathDirection d1, PathDirection d2) => !(d1 == d2);
            public override bool Equals(object obj)
            {
                return this == (obj as PathDirection);
            }
            public override int GetHashCode()
            {
                return (DRow << 16) | DCol;
            }
            public static PathDirection[] Next = new PathDirection[] {
                new PathDirection(0, -1),
                new PathDirection(-1, 0),
                new PathDirection(0, +1),
                new PathDirection(+1, 0)
            };
        }
        protected class MapGrid
        {
            public MapGrid(int row, int col)
            {
                this.HasButton = false;
                Row = row;
                Col = col;
            }
            public bool HasButton { get; set; } //判断连连看按钮，是否已经消除。
            public int Row { get; set; }  //行
            public int Col { get; set; }  //列
            public int Flag { get; set; } //标号
            /*计算路径用*/
            public MapGrid PrevOnPath { get; set; } //路径的上一个格子
            public MapGrid NextGrid { get; set; } //路径的格子
        }

        #region 属性变量

        private int NumOfGridDefaultChildren { get; }
        private LianLianKanButton[] _pairButton = new LianLianKanButton[2];
        private MapGrid[,] _mapReflex;

        /// <summary>
        /// 行数
        /// </summary>
        public int MapRow => _mapReflex.GetLength(0);
        /// <summary>
        /// 列数
        /// </summary>
        public int MapCol => _mapReflex.GetLength(1);

        /// <summary>
        /// 剩余图标数
        /// </summary>
        private int _remainNumberOfButton;

        /// <summary>
        /// 最大拐弯的次数
        /// </summary>
        public int MaxNumberOfCorner { get; } = 2;

        /// <summary>
        /// 游戏结束事件
        /// </summary>
        public static readonly RoutedEvent GameOverEvent =
            EventManager.RegisterRoutedEvent(nameof(GameOver), RoutingStrategy.Bubble
                , typeof(RoutedEventHandler)
                , typeof(LianLianKanMap));
        public event RoutedEventHandler GameOver
        {
            add
            {
                AddHandler(GameOverEvent, value);
            }
            remove
            {
                RemoveHandler(GameOverEvent, value);
            }
        }

        #endregion

        #region 方法

        private void CleanMap()
        {
            //禁止Remove 基本孩子
            int count = mapGrid.Children.Count;
            while (count > NumOfGridDefaultChildren)
            {
                count--;
                (mapGrid.Children[count] as LianLianKanButton).Checked -= OnCheckedLianLianButton;
                mapGrid.Children.RemoveAt(count);

            }

        }

        private void InitMapBackground(int mapRow, int mapCol)
        {
            //清理旧按钮
            this.CleanMap();
            //判断行列大小是否没变
            if (mapGrid.ColumnDefinitions.Count == mapCol
                && mapGrid.RowDefinitions.Count == mapRow)
            {
                //不需要改变
                return;
            }
            //清理旧格子
            mapGrid.ColumnDefinitions.Clear();
            mapGrid.RowDefinitions.Clear();
            //格子行列
            //初始化地图映像。
            _mapReflex = new MapGrid[mapRow, mapCol];
            for (int row = 0; row < mapRow; row++)
                for (int col = 0; col < mapCol; col++)
                    _mapReflex[row, col] = new MapGrid(row, col);
            //Grid控件，行
            for (int row = 0; row < mapRow; row++)
                mapGrid.RowDefinitions.Add(new RowDefinition());
            //Grid控件，列
            for (int col = 0; col < mapCol; col++)
                mapGrid.ColumnDefinitions.Add(new ColumnDefinition());
            //设置背景色，及显示分界线
            mapGrid.ShowGridLines = true;
            mapGrid.Background = new SolidColorBrush(Colors.Wheat);
            //镶入前景
            mapGridForeground.ColumnDefinitions.Clear();
            mapGridForeground.RowDefinitions.Clear();
            int foreRow = mapRow - 2, foreCol = mapCol - 2;
            for (int row = 0; row < foreRow; row++)
                mapGridForeground.RowDefinitions.Add(new RowDefinition());
            for (int col = 0; col < foreCol; col++)
                mapGridForeground.ColumnDefinitions.Add(new ColumnDefinition());
            Grid.SetRow(mapGridForeground, 1);
            Grid.SetColumn(mapGridForeground, 1);
            Grid.SetRowSpan(mapGridForeground, foreRow);
            Grid.SetColumnSpan(mapGridForeground, foreCol);
            mapGridForeground.Background = new SolidColorBrush(Colors.Tan);
        }

        public void InitMap(int row, int col)
        {
            //判断异常，行列必须有一个是偶数。
            if (1 == row % 2 && 1 == row % 2)
                throw new Exception("The rowAndCol of map must be Even Number !");
            //初始化背景
            InitMapBackground(row, col);
        }

        public void ResetGame()
        {
            int mapRow = this.MapRow, mapCol = MapCol;
            //先停止游戏
            StopGame();
            //重置地图映像
            for (int row = 0; row < mapRow; row++)
                for (int col = 0; col < mapCol; col++)
                    _mapReflex[row, col].HasButton = false;
            //添加连连看按钮
            _remainNumberOfButton = (this.MapRow - 2) * (this.MapCol - 2);
            var random = new Random(DateTime.Now.Millisecond);
            for (int num = 0; num < _remainNumberOfButton; num += 2)
            {
                int pairID = random.Next(LianLianKanBrush.NumberOfBrush);
                for (int count = 0; count < 2; count++)
                {
                    int localRow, localCol;
                    do
                    {
                        localRow = random.Next(1, mapRow - 1);
                        localCol = random.Next(1, mapCol - 1);
                    } while (_mapReflex[localRow, localCol].HasButton == true);
                    var button = new LianLianKanButton(pairID, localRow, localCol,
                        LianLianKanBrush.GetSolidColorBrush(pairID));
                    button.ToolTip = $"{pairID}";
                    button.Checked += OnCheckedLianLianButton;
                    Grid.SetRow(button, localRow);
                    Grid.SetColumn(button, localCol);
                    mapGrid.Children.Add(button);
                    _mapReflex[localRow, localCol].HasButton = true;
                    _mapReflex[localRow, localCol].Row = localRow;
                    _mapReflex[localRow, localCol].Col = localCol;
                }//for
            }//for
        }

        public void StartGame()
        {
            ResetGame();
        }

        public void StopGame()
        {
            this.CleanMap();
        }

        public void OnCheckedLianLianButton(object sender, RoutedEventArgs e)
        {
            LianLianKanButton button = sender as LianLianKanButton;
            if (_pairButton[0] != null)
            {
                _pairButton[1] = button;
                _pairButton[0].IsChecked = false;
                _pairButton[1].IsChecked = false;
                //判断是否是一对
                bool success = false;
                if (_pairButton[0].PairID == _pairButton[1].PairID)
                {
                    //只有是一对时，才去找路径。
                    var path = this.FindPath(_mapReflex[_pairButton[0].Row, _pairButton[0].Col],
                        _mapReflex[_pairButton[1].Row, _pairButton[1].Col]);
                    if (path != null)
                    {
                        _pairButton[0].Checked -= OnCheckedLianLianButton;
                        _pairButton[1].Checked -= OnCheckedLianLianButton;
                        mapGrid.Children.Remove(_pairButton[0]);
                        mapGrid.Children.Remove(_pairButton[1]);
                        _mapReflex[_pairButton[0].Row, _pairButton[0].Col].HasButton = false;
                        _mapReflex[_pairButton[1].Row, _pairButton[1].Col].HasButton = false;
                        _remainNumberOfButton -= 2;
                        if (_remainNumberOfButton == 0)
                        {
                            GameOverSound.Play();
                            RaiseEvent(new RoutedEventArgs(GameOverEvent));
                        }
                        else
                            succeedToMatchSound.Play();
                        success = true;
                    }
                }
                _pairButton[0] = null;
                _pairButton[1] = null;
                //播放声音
                if (success == false)
                    failToMatchSound.Play();
            }
            else
                _pairButton[0] = button;
        }

        private MapGrid[] FindPathByRecursion(MapGrid source, MapGrid end)
        {
            if (source.Row == end.Row && source.Col == end.Col)
                return null;
            int numberOfRow = _mapReflex.GetLength(0);
            int numberOfCol = _mapReflex.GetLength(1);
            for (int row = 0; row < this.MapRow; row++)
                for (int col = 0; col < this.MapCol; col++)
                    _mapReflex[row, col].PrevOnPath = null;
            //先填充source的PrevOnPath
            source.PrevOnPath = source;
            if (FindPathByRecursion(end, source.Row, source.Col, 0, new PathDirection(0, 0)))
            {
                //找到路径后，还原source的PrevOnPath
                source.PrevOnPath = null;
                //找到路径
                var stack = new Stack<MapGrid>();
                var nextGrid = end;
                while (nextGrid != null)
                {
                    stack.Push(nextGrid);
                    nextGrid = nextGrid.PrevOnPath;
                }
                MapGrid[] result = new MapGrid[stack.Count];
                int index = 0;
                while (stack.Count > 0)
                    result[index++] = stack.Pop();
                return result;
            }
            return null;
        }

        private bool FindPathByRecursion(MapGrid end, int currRow, int currCol, int currNumOfCorner, PathDirection currDir)
        {
            int nextRow, nextCol;
            int corner = 0;
            PathDirection dir;
            //四个方向
            for (int count = 0; count < PathDirection.Next.Count(); count++)
            {
                dir = PathDirection.Next[count];
                nextRow = currRow + dir.DRow;
                nextCol = currCol + dir.DCol;
                //判断是否越界
                if (nextRow < 0 || nextRow >= this.MapRow
                     || nextCol < 0 || nextCol >= this.MapCol)
                    continue;
                //判断是否已经路过，为null则没路过
                if (this._mapReflex[nextRow, nextCol].PrevOnPath == null)
                {
                    //是否拐弯
                    corner = currDir == dir ? 0 : 1;
                    //连接上
                    this._mapReflex[nextRow, nextCol].PrevOnPath = this._mapReflex[currRow, currCol];
                    //拐弯次数没有超过上限，
                    if (currNumOfCorner + corner <= this.MaxNumberOfCorner)
                    {
                        //下一个格子不含有按钮，则进入下个格子。
                        if (this._mapReflex[nextRow, nextCol].HasButton == false)
                        {
                            //下一个格子
                            if (true == FindPathByRecursion(end, nextRow, nextCol, currNumOfCorner + corner, dir))
                                return true;
                        }
                        //下一个是目的地
                        else if (nextRow == end.Row && nextCol == end.Col)
                            return true;
                    }
                    //此路不通，还原
                    this._mapReflex[nextRow, nextCol].PrevOnPath = null;
                }
            }
            return false;
        }

        /// <summary>
        /// 寻路 
        /// </summary>
        private MapGrid[] FindPath(MapGrid source, MapGrid end)
        {
        //首先从起点，走向终点
        if (this.GoToTheEnd(source, end))
        {
            //然后回溯到起点
            if (this.GoToTheSource(source, end))
            {
                //找到了符合条件的路径
                var queue = new Queue<MapGrid>();
                var nextGrid = source;
                while (nextGrid != null)
                {
                    queue.Enqueue(nextGrid);
                    nextGrid = nextGrid.NextGrid;
                }
                //返回路径
                return queue.ToArray();
            }
        }
        return null;
        }


        /// <summary>
        /// 向起点移动
        /// </summary>
        private bool GoToTheSource(MapGrid source, MapGrid end)
        {
        //初始化
        int numberOfRow = _mapReflex.GetLength(0);
        int numberOfCol = _mapReflex.GetLength(1);
        for (int x = 0; x < numberOfRow; x++)
            for (int y = 0; y < numberOfCol; y++)
                _mapReflex[x, y].NextGrid = null;
        //进入递归回溯
        return GoToTheSource(source, end, 0, new PathDirection(0, 0));
        }

        /// <summary>
        /// 向起点移动（用于递归寻）
        /// </summary>
        private bool GoToTheSource(MapGrid source, MapGrid current, int currNumOfCorner, PathDirection currDir)
        {
        if (currNumOfCorner > this.MaxNumberOfCorner)
            //拐弯次数太多
            return false;
        if (current.Row == source.Row && current.Col == source.Col)
            //到达了起点
            return true;
        //
        int nextRow, nextCol;
        PathDirection dir;
        //四个方向
        for (int count = 0; count < PathDirection.Next.Count(); count++)
        {
            dir = PathDirection.Next[count];
            nextRow = current.Row + dir.DRow;
            nextCol = current.Col + dir.DCol;
            //判断是否越界
            if (nextRow < 0 || nextRow >= this.MapRow
                    || nextCol < 0 || nextCol >= this.MapCol)
                continue;
            var grid = this._mapReflex[nextRow, nextCol];
            //判断是否是返回路线
            if (current.Flag > grid.Flag)
            {
                //记录路线
                grid.NextGrid = current;
                //继续下一步
                if (GoToTheSource(source, grid, currNumOfCorner + (currDir == dir ? 0 : 1), dir))
                    //到达起点了
                    return true;
            }
        }
        return false;
        }

        /// <summary>
        /// 向终点移动
        /// </summary>
        private bool GoToTheEnd(MapGrid source, MapGrid end)
        {
            //初始化
            int numberOfRow = _mapReflex.GetLength(0);
            int numberOfCol = _mapReflex.GetLength(1);
            for (int x = 0; x < numberOfRow; x++) {
                for (int y = 0; y < numberOfCol; y++)
                    _mapReflex[x, y].Flag = 0;
            }
            source.Flag = 1;
            //入队列
            var queue = new Queue<MapGrid>();
            queue.Enqueue(source);
            //循环
            while (queue.Count > 0)
            {
                var currGrid = queue.Dequeue();
                int nextRow, nextCol;
                PathDirection dir;
                //四个方向
                for (int count = 0; count < PathDirection.Next.Count(); count++)
                {
                    dir = PathDirection.Next[count];
                    nextRow = currGrid.Row + dir.DRow;
                    nextCol = currGrid.Col + dir.DCol;
                    //判断是否越界
                    if (nextRow < 0 || nextRow >= this.MapRow || nextCol < 0 || nextCol >= this.MapCol)
                        continue;
                    var grid = this._mapReflex[nextRow, nextCol];
                    //判断是否已经路过，或者是否有按钮
                    if (grid.Flag == 0)
                    {   
                        //判断是否有对子
                        if (grid.HasButton == false)
                        {   //设置对子的步数
                            grid.Flag = currGrid.Flag + 1;
                            queue.Enqueue(grid);
                        }
                        else if (grid.Row == end.Row && grid.Col == end.Col)
                            //如果是终点，只设置对子的步数
                            grid.Flag = currGrid.Flag + 1;
                    }
                }
            }
            return end.Flag != 0;
        }

        /// <summary>
        /// 当声音停止时复原
        /// </summary>
        private void OnMediaEnded(object sender, RoutedEventArgs e)
        {
            var media = sender as MediaElement;
            media.Stop();
            media.Position = TimeSpan.FromSeconds(0);
        }

        #endregion
    }
}
