using UnityEngine;
using UnityEngine.UI;

namespace ScriptEraser
{
    public class Eraser : MonoBehaviour
    {
        private Material _mat;                      //要使用的材质
        private RectTransform _rt;                   //要被擦除的ui
        public Texture texture;                    //ui绘制的贴图
        public Texture brush;
        private RenderTexture _after;               //为材质提供的实时贴图
        private RenderTexture _before;              //过渡贴图变量
        public bool _active = false;
        private int _frequency = 8;
        public Camera stageCamera;                 //UI渲染摄像机，基于这个摄像机做触摸点检测
        private float _threshold = 0.8f;            //像素点擦除比例大于这个值就认为擦除成功
        private Vector2 _lastPos;
        private bool _haveLastPos = false;

        private void Start()
        {
            //stageCamera = GameObject.Find("Stage Camera").GetComponent<Camera>();
            var image = GetComponent<Image>();
            _mat = new Material(Shader.Find("Custom/EraserShader"));
            image.sprite = null;
            image.material = _mat;

            _rt = GetComponent<RectTransform>();
            int width = (int)_rt.rect.width; int height = (int)_rt.rect.height;
            _after = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
            _before = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);

            Graphics.Blit(texture, _after);
            Graphics.Blit(texture, _before);
            _mat.SetTexture("_MainTex", _after);           //初始绘制贴图
            _mat.SetTexture("_Brush", brush);           //初始绘制贴图
            _mat.SetInt("_BrushWidth", brush.width);           //初始绘制贴图
            _mat.SetInt("_BrushHeight", brush.height);           //初始绘制贴图
        }
        private void Update()
        {
            if (!_active) {
                return;
            }
            if (Input.GetMouseButton(0))
            {
                var pos = Vector2.zero;
                //获取相对于该贴图的像素纹理坐标
                RaycastHit hitInfo;
                var ray = stageCamera.ScreenPointToRay(Input.mousePosition);
                //Debug.DrawRay(ray.origin, ray.direction *100, Color.red);
                if (Physics.Raycast(ray, out hitInfo, 50))
                {
                    if (hitInfo.collider.name.Equals(this.name))
                    {
                        var posPoint = hitInfo.point;
                        RectTransformUtility.ScreenPointToLocalPointInRectangle(_rt, posPoint, null, out pos);
                        pos = pos + new Vector2(_rt.rect.width, _rt.rect.height) / 2;
                        if (_haveLastPos && Vector2.Distance(pos, _lastPos) > 2)
                        {
                            var step = Vector2.Distance(pos, _lastPos) / 2;
                            var curPos = _lastPos;
                            while (Vector2.Distance(curPos, pos) > 2)
                            {
                                _draw((int)curPos.x, (int)curPos.y);
                                curPos.x = (pos.x - _lastPos.x) / step + curPos.x;
                                curPos.y = (pos.y - _lastPos.y) / step + curPos.y;
                            }
                        }
                        else
                        {
                            _draw((int)pos.x, (int)pos.y);
                        }
                        _haveLastPos = true;
                        _lastPos = pos;
                    }
                    }
                }
            else if(Input.GetMouseButtonUp(0))
                {
                _haveLastPos = false;
            }
        }

        private void _draw(int posX, int posY)
        {
            _mat.SetInt("_CenterX", posX);
            _mat.SetInt("_CenterY", posY);
            Graphics.Blit(_before, _after, _mat);
            Graphics.Blit(_after, _before);
        }

        public void SetActive(bool active)
        {
            _active = active;
        }

        /// <summary>
        /// 是否绘制完成
        /// 绘制成功可以将_after拷贝到texture2D上,通过遍历像素点的透明度来判断贴图是否擦除完成
        /// 成功后通过Release()方法来释放_after/_before空间资源
        /// </summary>
        /// <returns></returns>
        public bool IsCompleted()
        {
            int width = _after.width;
            int height = _after.height;
            Texture2D texture2D = new Texture2D(width, height, TextureFormat.ARGB32, false);
            RenderTexture.active = _after;
            texture2D.ReadPixels(new Rect(0, 0, width, height), 0 ,0);
            texture2D.Apply();
            RenderTexture.active = null;
            var pixels = texture2D.GetPixels32();
            var completedPixelNum = 0;
            var allPixelNum = 0;
            for (int i = 0; i < width; i+= _frequency)
            {
                for (int j = 0; j < height; j += _frequency)
                {
                    allPixelNum += 1;
                    if (pixels[i * height + j].a <= 200)
                    {
                        completedPixelNum += 1;
                    }
                }
            }
            var ratio = (float)completedPixelNum / (float)allPixelNum;
            if (ratio >= _threshold)
            {
                return true;
            }
            return false;
        }    
    }
}
