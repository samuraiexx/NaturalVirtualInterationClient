using UnityEngine;

public class TextureUtils {
  static public void drawCircle(Texture2D tex, Vector2 p, int r, Color col) {
    int x, y, px, nx, py, ny, d;

    for (x = 0; x <= r; x++)
    {
      d = (int)Mathf.Round(Mathf.Sqrt(r * r - x * x));
      for (y = 0; y <= d; y++)
      {
        px = (int) p.x + x;
        nx = (int) p.x - x;
        py = (int) p.y + y;
        ny = (int) p.y - y;

        tex.SetPixel(px, py, col);
        tex.SetPixel(nx, py, col);

        tex.SetPixel(px, ny, col);
        tex.SetPixel(nx, ny, col);
      }
    }
  }

  public static void drawLine(
    Texture2D tex,
    Vector2 p1,
    Vector2 p2,
    Color col,
    float thickness = 1
  ) {
    Vector2 t = p1;
    Vector2 delta = (p2 - p1);
    Vector2 normal = (new Vector2(delta.y, -delta.x)).normalized;

    float step = 1/delta.magnitude;
    float ctr = 0;

    while (ctr < 1)
    {
      t = Vector2.Lerp(p1, p2, ctr);
      ctr += step;

      var p1N = t - normal*thickness/2;
      var p2N = t + normal*thickness/2;
      float ctrN = 0;
      while (ctrN < 1)
      {
        ctrN += step;

        var u = Vector2.Lerp(p1N, p2N, ctrN);
        tex.SetPixel((int)u.x, (int)u.y, col);
      }
    }
  }
}