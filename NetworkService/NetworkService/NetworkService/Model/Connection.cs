using NetworkService.MVVM;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Controls;

namespace NetworkService.Model
{
    public class Connection : BindableBase
    {
        #region Fields
        private PowerConsumptionEntity fromEntity;
        private PowerConsumptionEntity toEntity;
        private Canvas fromCanvas;
        private Canvas toCanvas;
        private Line visualLine;
        private bool isValid;
        #endregion

        #region Properties

        public PowerConsumptionEntity FromEntity
        {
            get { return fromEntity; }
            set { SetProperty(ref fromEntity, value); }
        }

        public PowerConsumptionEntity ToEntity
        {
            get { return toEntity; }
            set { SetProperty(ref toEntity, value); }
        }

        public Canvas FromCanvas
        {
            get { return fromCanvas; }
            set { SetProperty(ref fromCanvas, value); }
        }

        public Canvas ToCanvas
        {
            get { return toCanvas; }
            set { SetProperty(ref toCanvas, value); }
        }
        public Line VisualLine
        {
            get { return visualLine; }
            set { SetProperty(ref visualLine, value); }
        }

        public bool IsValid
        {
            get { return isValid; }
            set { SetProperty(ref isValid, value); }
        }

        public string ConnectionId
        {
            get
            {
                if (FromEntity != null && ToEntity != null)
                    return $"{FromEntity.Id}-{ToEntity.Id}";
                return string.Empty;
            }
        }

        #endregion

        #region Constructors

        public Connection()
        {
            InitializeLine();
        }

        public Connection(PowerConsumptionEntity fromEntity, PowerConsumptionEntity toEntity, Canvas fromCanvas, Canvas toCanvas)
        {
            FromEntity = fromEntity;
            ToEntity = toEntity;
            FromCanvas = fromCanvas;
            ToCanvas = toCanvas;
            IsValid = true;

            InitializeLine();
            UpdateLinePosition();
        }

        #endregion

        #region Methods

        private void InitializeLine()
        {
            VisualLine = new Line
            {
                Stroke = Brushes.Black,
                StrokeThickness = 2,
                StrokeDashArray = null, 
                IsHitTestVisible = false 
            };
        }

        public void UpdateLinePosition()
        {
            if (FromCanvas == null || ToCanvas == null || VisualLine == null)
                return;

            var fromPosition = GetCanvasPosition(FromCanvas);
            var toPosition = GetCanvasPosition(ToCanvas);

            var fromCenter = GetCanvasPosition(FromCanvas);
            var toCenter = GetCanvasPosition(ToCanvas);


            VisualLine.X1 = fromCenter.X;
            VisualLine.Y1 = fromCenter.Y;
            VisualLine.X2 = toCenter.X;
            VisualLine.Y2 = toCenter.Y;

            UpdateLineColor();
        }

        private System.Windows.Point GetCanvasPosition(Canvas canvas)
        {
            if (canvas.Parent is Grid parentGrid)
            {
                int row = Grid.GetRow(canvas);
                int column = Grid.GetColumn(canvas);

                double canvasWidth = 120; // 
                double canvasHeight = 144; // Margin + height (3 * 8(MarginTop) = 24 Additional)

                return new System.Windows.Point(
                    column * canvasWidth + 60, // Centar of Canvas
                    row * canvasHeight + 72    // Centar of Canvas
                );
            }

            return new System.Windows.Point(0, 0);
        }

        public void UpdateLineColor()
        {
            if (VisualLine == null || FromEntity == null || ToEntity == null)
                return;

            if (FromEntity.IsValueValid && ToEntity.IsValueValid)
            {
                VisualLine.Stroke = Brushes.Green; // Normal
                VisualLine.StrokeThickness = 2; 
            }
            else
            {
                VisualLine.Stroke = Brushes.Red; // Alert
                VisualLine.StrokeThickness = 3; 
            }
        }

        #endregion
    }
}