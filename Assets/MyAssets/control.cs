using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class control : MonoBehaviour
{
    private Material _Mat;                      //要使用的材质
    private RectTransform rt;                   //要被擦除的ui
    public Texture _Texture;                    //ui绘制的贴图
    private RenderTexture _After;               //为材质提供的实时贴图
    private RenderTexture _Before;              //过渡贴图变量
    void Start()
    {
        rt = GetComponent<RectTransform>();                

        _Mat = new Material(Shader.Find("Hidden/Brush"));

        rt.GetComponent<Image>().material = _Mat;

        _After = new RenderTexture((int)rt.rect.width, (int)rt.rect.height, 0, RenderTextureFormat.ARGB32);
        _Before = new RenderTexture((int)rt.rect.width, (int)rt.rect.height, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(_Texture, _After);
        Graphics.Blit(_Texture, _Before);
        _Mat.SetTexture("_MainTex", _After);           //初始绘制贴图
    }
    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            var pos = Vector2.zero;

            //获取相对于该贴图的像素纹理坐标
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, Input.mousePosition, null, out pos);
            pos = pos + new Vector2(rt.rect.width, rt.rect.height) / 2;

            _Mat.SetInt("_CenterX", (int)pos.x);
            _Mat.SetInt("_CenterY", (int)pos.y);
            Graphics.Blit(_Before, _After, _Mat);       
            Graphics.Blit(_After, _Before);
        }
    }

    //绘制成功可以将_After拷贝到texture2D上,通过遍历像素点的透明度来判断贴图是否擦除完成
    //成功后通过Release()方法来释放_After/_Before空间资源
    
}
