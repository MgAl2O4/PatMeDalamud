using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures.TextureWraps;
using System;
using System.Numerics;

namespace PatMe
{
    public class SplashScreenUI : IDisposable
    {
        private enum AnimPhase
        {
            None,
            Appear,
            Keep,
            Disappear,
        }

        private AnimPhase anim = AnimPhase.None;
        private static readonly float[] animDuration = [0.0f, 1.0f, 1.0f, 1.0f];
        private float animTimeRemaining = 0.0f;

        public void Show()
        {
            SetAnim(AnimPhase.Appear);
        }

        public void Draw()
        {
            if (anim == AnimPhase.None)
            {
                return;
            }

            var overlayImage = GetOverlayImage();
            if (overlayImage == null)
            {
                return;
            }

            float animPct = 1.0f - Math.Max(0.0f, animTimeRemaining / animDuration[(int)anim]);

            // draw image
            {
                var viewport = ImGui.GetMainViewport();
                var viewportCenter = viewport.GetCenter();
                var drawHalfSize = new Vector2(overlayImage.Width * 0.5f, overlayImage.Height * 0.5f);

                if (drawHalfSize.X > viewportCenter.X || drawHalfSize.Y > viewportCenter.Y)
                {
                    drawHalfSize.Y = viewportCenter.Y * 3 / 4;
                    drawHalfSize.X = overlayImage.Width * drawHalfSize.Y / overlayImage.Height;
                }

                if (anim == AnimPhase.Appear)
                {
                    drawHalfSize *= AnimElastic(animPct);
                }

                var drawAlpha =
                    (anim == AnimPhase.Appear) ? 1 - Math.Pow(1 - animPct, 4) :
                    (anim == AnimPhase.Disappear) ? (1.0f - animPct) :
                    1.0f;

                uint drawColor = 0xffffff | (uint)(drawAlpha * 255) << 24;

                var drawList = ImGui.GetForegroundDrawList();
                drawList.AddImage(overlayImage.Handle, viewportCenter - drawHalfSize, viewportCenter + drawHalfSize, Vector2.Zero, Vector2.One, drawColor);
            }

            // state transitions
            animTimeRemaining -= ImGui.GetIO().DeltaTime;
            if (animTimeRemaining <= 0.0f)
            {
                if (anim == AnimPhase.Disappear)
                {
                    SetAnim(AnimPhase.None);
                }
                else
                {
                    SetAnim(anim + 1);
                }
            }
        }

        private float AnimElastic(float alpha)
        {
            const float c4 = (float)(2 * Math.PI) / 3.0f;

            return (alpha == 0) ? 0.0f :
              (alpha == 1) ? 1.0f :
              (float)(Math.Pow(2, -10 * alpha) * Math.Sin((alpha * 10 - 0.75) * c4) + 1);
        }

        private void SetAnim(AnimPhase anim)
        {
            this.anim = anim;
            animTimeRemaining = animDuration[(int)anim];
        }

        public void Dispose()
        {
        }

        private IDalamudTextureWrap? GetOverlayImage()
        {
            var myAssembly = GetType().Assembly;
            var myAssemblyName = myAssembly.GetName().Name;
            var resourceName = $"{myAssemblyName}.assets.fan-kit-lala.png";

            var textureResource = Service.textureProvider.GetFromManifestResource(myAssembly, resourceName);
            if (textureResource != null)
            {
                var hasWrap = textureResource.TryGetWrap(out IDalamudTextureWrap? resultImage, out Exception? ex);
                if (hasWrap)
                {
                    return resultImage;
                }
            }

            return null;
        }

    }
}
