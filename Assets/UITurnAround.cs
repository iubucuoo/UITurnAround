using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UITurnAround : MonoBehaviour
{
    public Graphic ui;
    public float Speed = 5;
    public float SmoothTime = 0.1f;
    public int NowListIndex;
    public System.Action<int> SelectChange;
    public System.Action<int> CloseUI;

    public int SleIndex
    {
        get { return _selindex; }
        set {
            _selindex = value;
            CheckEluerZ = OneGridAngle * _selindex;
            localEluer.z = CheckEluerZ;
            ui.transform.localEulerAngles = localEluer;
        }
    }
    private int _selindex;
    private float OneGridAngle = 36;//一格的角度
    private float CacheAng2Index;
    private float CurrentVelocity;
    private float CheckEluerZ;
    private float RotateAngle;
    private bool IsHide;
    private bool IsSelect = false;
    private bool IsChangeV = false;
    private Vector3 localEluer; //模型欧拉角存储变量
    private Vector2 ModelPos;
    private Vector2 mousePos; //当前鼠标位置
    private Vector2 cacheMousePos;
    private Vector2 premousePos;//上一帧鼠标位置
    private Quaternion q;
    private List<Transform> Grids = new List<Transform>();
    void Start()
    {
        ModelPos = ui.GetUIScreenPosition();
        //Debug.LogError("图片坐标    " +go.transform.position+"   " + ModelPos);
        ui.transform.localEulerAngles = localEluer;
        var count = ui.transform.childCount;
        for (int i = 0; i < count; i++)
        {
            Grids.Add(ui.transform.GetChild(i).Find("icon"));
        }
    }
    Vector3 GetTouchPos()
    {
#if UNITY_EDITOR
        return Input.mousePosition;
#else
        if (Input.touchCount > 0)
        {
            return Input.GetTouch(0).position;
        }
        else
        {
            return Vector3.zero;
        }
#endif
    }
    public virtual void LateUpdate()
    {

#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0) )
#else
        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began )
#endif
        {
            IsHide = false;
            Vector3 touchpos = GetTouchPos();
            //Debug.LogError("点击坐标    "+ touchpos);
            if (ui.IsBeenTouch( touchpos))
            {
                IsSelect = true;
                premousePos = mousePos = touchpos; //每次重新点击的时候都重置鼠标上一帧点击坐标
                CheckEluerZ = localEluer.z;
            }
            else
            {
                //Debug.LogError("准备隐藏界面");
                IsHide = true;//关闭界面
            }
        }
#if UNITY_EDITOR
        if (Input.GetMouseButton(0)  )
#else
        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved  )
#endif
        {
            if (IsSelect)
            {
                mousePos = GetTouchPos();
                RotateAngle = Vector2.Angle(premousePos - ModelPos, mousePos - ModelPos);
                //Debug.LogError(RotateAngle);
                if (RotateAngle == 0)
                {
                    premousePos = mousePos;
                }
                else
                {
                    IsChangeV = true;
                    q = Quaternion.FromToRotation(premousePos - ModelPos, mousePos - ModelPos);
                    float k = q.z > 0 ? 1 : -1;
                    float z = localEluer.z + k * RotateAngle * Speed;
                    CheckEluerZ = z;
                    premousePos = mousePos;
                }
            }
            else
            { cacheMousePos = GetTouchPos(); }
        }
#if UNITY_EDITOR
        if (Input.GetMouseButtonUp(0))
#else
        if (Input.touchCount != 1)
#endif
        {
            if (IsSelect && IsChangeV)
            {
                CheckEluerZ = GetEndEuler();
            }
            if (IsHide &&  !ui.IsBeenTouch(cacheMousePos))
            {
                //不显示界面
                IsHide = false;
                //Debug.LogError("隐藏");
                CloseUI?.Invoke(NowListIndex);
            }
            IsSelect = false;
            IsChangeV = false;
        }
        localEluer.z = Mathf.SmoothDamp(localEluer.z, CheckEluerZ, ref CurrentVelocity, SmoothTime);
        ui.transform.localEulerAngles = localEluer;
        UpGridsIconRotation();
        UpSleIndex();
    }

    float GetEndEuler()
    {
        float cacheEZ = CheckEluerZ;
        float angle = OneGridAngle;
        float harfangle = OneGridAngle*0.5f;
        float retnum = 0;
        if (cacheEZ>=0)
        {
            while (!(cacheEZ>=0 && cacheEZ<= angle))
            {
                cacheEZ -= angle;
            }
            //大于harfangle 加   小于harfangle 减   
            if (cacheEZ> harfangle)
            {
                retnum = CheckEluerZ + (angle - cacheEZ);
            }
            else
            {
                retnum = CheckEluerZ - cacheEZ ;
            }
        }
        else
        {
            while (!(cacheEZ >= -angle && cacheEZ < 0))
            {
                cacheEZ += angle;
            }
            // 大于-harfangle 加(加负数)  小于-harfangle  减
            if (cacheEZ > -harfangle)
            {
                retnum = CheckEluerZ - cacheEZ;
            }
            else
            {
                retnum = CheckEluerZ - (angle + cacheEZ);
            }
        }
        //Debug.LogError(CheckEluerZ+"  :  "+ ang+"   :   "+ cacheEZ + "  :  "+ retnum);
        return retnum;
    }
    float LimitAngle(float angle, int nMinAngle, int nMaxAngle, int type = 0)
    {
        while (angle < nMinAngle)
        {
            if (type == 1)
                return nMinAngle;
            angle += 360;
        }

        while (angle >= nMaxAngle)
        {
            if (type == 1)
                return nMaxAngle - 1;
            angle -= 360;
        }

        return angle;
    }
    void UpSleIndex()
    {
        float ang = LimitAngle(CheckEluerZ, 0, 360);
        if (ang!= CacheAng2Index)
        {
            int index = (int)(ang / OneGridAngle);
            if (NowListIndex != index)
            {
                NowListIndex = index;
                //Debug.Log(NowListIndex);
                SelectChange?.Invoke(NowListIndex);
            }
            CacheAng2Index = ang;
        }
    }
    void UpGridsIconRotation()
    {
        for (int i = 0; i < Grids.Count; i++)
        {
            Grids[i].rotation = Quaternion.identity;
        }
    }
    //获取UI的屏幕坐标【0,1】




}
public static class UITouchEctend
{
    public static Vector2 GetUIScreenPosition(this Graphic ui)
    {
        Canvas canvas = ui.GetComponentInParent<Canvas>();
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay ||
        canvas.renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera == null)
        {
            float x = ui.transform.position.x;
            float y = ui.transform.position.y;
            return new Vector2(x, y);
        }
        //ScreenSpaceCamera 和 WorldSpace模式  注意WorldSpace没有关联UI相机获取到的就会有问题
        else
        {
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, ui.transform.position);
            float x = screenPos.x;
            float y = screenPos.y;
            return new Vector2(x, y);
        }
    }
    public static bool IsBeenTouch(this Graphic _Ui,  Vector3 touchpos)
    {
        bool isInRect = false;
        float _Width = _Ui.GetComponent<RectTransform>().rect.width;//获取ui的实际宽度
        float _Hight = _Ui.GetComponent<RectTransform>().rect.height;//长度
        Vector3 uiPos =  _Ui.GetUIScreenPosition();
        //Debug.Log(uiPos + "    " + touchpos);
        if (touchpos.x < (uiPos.x + _Width / 2) && touchpos.x > (uiPos.x - _Width / 2) &&
            touchpos.y < (uiPos.y + _Hight / 2) && touchpos.y > (uiPos.y - _Hight / 2))
        {
            isInRect = true;
        }
        return isInRect;
    }
}
