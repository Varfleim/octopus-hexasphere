
using UnityEngine;

namespace HS
{
    public class TextureScaler : MonoBehaviour
    {
        public static void Scale(
            Texture2D texture,
            int width, int height,
            FilterMode mode = FilterMode.Trilinear)
        {
            //Берём активную текстуру
            RenderTexture currentActiveRT = RenderTexture.active;

            //Создаём прямоугольник как холст
            Rect texRect = new Rect(0, 0, width, height);
            //Растягиваем переданную текстуру
            _gpu_scale(
                texture,
                width, height,
                mode);

            //Обновляем текстуру
            texture.Reinitialize(width, height);
            texture.ReadPixels(texRect, 0, 0, true);
            texture.Apply(true);

            //Возвращаем текстуру
            RenderTexture.active = currentActiveRT;
        }

        static void _gpu_scale(
            Texture2D source,
            int width, int height,
            FilterMode fmode)
        {
            //"Нам нужна исходная текстура в VRAM, потому что мы рендерим с помощью неё"
            //We need the source texture in VRAM because we render with it
            source.filterMode = fmode;
            source.Apply(true);

            //"Используем RTT для лучших качества и производительности"
            //Using RTT for best quality and performance. Thanks, Unity 5
            RenderTexture rtt = new RenderTexture(
                width, height,
                32);

            //"Устанавливаем RTT в порядок, чтобы рендерить в него"
            //Set the RTT in order to render to it
            Graphics.SetRenderTarget(rtt);

            //"Устанавливаем 2D-матрицу в интервале 0..1, чтобы никому не нужно было беспокоиться о размере"
            //Setup 2D matrix in range 0..1, so nobody needs to care about sized
            GL.LoadPixelMatrix(0, 1, 1, 0);

            //"Затем очищаем и рисуем текстуру, чтобы заполнить RTT"
            //Then clear & draw the texture to fill the entire RTT.
            GL.Clear(true, true,
                new Color(0, 0, 0, 0));
            Graphics.DrawTexture(
                new Rect(0, 0, 1, 1),
                source);

        }
    }
}
