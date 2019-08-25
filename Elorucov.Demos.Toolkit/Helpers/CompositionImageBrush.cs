using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Composition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.DirectX;
using Windows.Graphics.Imaging;
using Windows.UI.Composition;

namespace Elorucov.Demos.Toolkit.Helpers {
    public class CompositionImageBrush : IDisposable {
        private CompositionImageBrush() {
        }
        public CompositionBrush Brush {
            get {
                return (this.drawingBrush);
            }
        }
        void CreateDevice(Compositor compositor) {
            this.graphicsDevice = CanvasComposition.CreateCompositionGraphicsDevice(
              compositor, CanvasDevice.GetSharedDevice());
        }
        void CreateDrawingSurface(Size drawSize) {
            this.drawingSurface = this.graphicsDevice.CreateDrawingSurface(
              drawSize,
              DirectXPixelFormat.B8G8R8A8UIntNormalized,
              DirectXAlphaMode.Premultiplied);
        }
        void CreateSurfaceBrush(Compositor compositor) {
            this.drawingBrush = compositor.CreateSurfaceBrush(
             this.drawingSurface);
        }
        public static CompositionImageBrush FromBGRASoftwareBitmap(
          Compositor compositor,
          SoftwareBitmap bitmap,
          Size outputSize) {
            CompositionImageBrush brush = new CompositionImageBrush();

            brush.CreateDevice(compositor);

            brush.CreateDrawingSurface(outputSize);
            brush.DrawSoftwareBitmap(bitmap, outputSize);
            brush.CreateSurfaceBrush(compositor);

            return (brush);
        }
        void DrawSoftwareBitmap(SoftwareBitmap softwareBitmap, Size renderSize) {
            using (var drawingSession = CanvasComposition.CreateDrawingSession(
              this.drawingSurface)) {
                using (var bitmap = CanvasBitmap.CreateFromSoftwareBitmap(drawingSession.Device,
                  softwareBitmap)) {
                    drawingSession.DrawImage(bitmap,
                      new Rect(0, 0, renderSize.Width, renderSize.Height));
                }
            }
        }
        public void Dispose() {
            // TODO: I'm unsure about the lifetime of these objects - is it ok for
            // me to dispose of them here when I've done with them and, especially,
            // the graphics device?
            this.drawingBrush.Dispose();
            this.drawingSurface.Dispose();
            this.graphicsDevice.Dispose();
        }
        CompositionGraphicsDevice graphicsDevice;
        CompositionDrawingSurface drawingSurface;
        CompositionSurfaceBrush drawingBrush;
    }
}
