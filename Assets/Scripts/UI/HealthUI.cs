using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using ZZZ.HealthData;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private Image healthImg;
    public CanvasGroup canvasGroup;
    //MVC
    //该层为V层，作为视图层（更新UI）
    //M层应该为生命值characterhealthInfo（存储各种有关生命值的信息，以及对于生命值数据的更新）
    //C层为PlayerHealthUIController（触发玩家反应的控制层，如果玩家扣血，那么C层会去通知M层，M层再去通知V层）



    public void UpdateHealthImg(CharacterHealthInfo healthInfo)
    {
        healthImg.fillAmount = healthInfo.CurrentHP/healthInfo.MaxHP;
    }


    // 提供给外部调用的“开始渐变消失”方法
    public void FadeOut(float duration)
    {
        if (canvasGroup != null)
        {
            StartCoroutine(FadeOutCoroutine(duration));
        }
    }

    // 执行渐变的协程
    private IEnumerator FadeOutCoroutine(float duration)
    {
        float elapsedTime = 0f;
        float startAlpha = canvasGroup.alpha;

        // 在指定的 duration（秒）内，将 alpha 从 1 降到 0
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / duration);
            yield return null; // 等待下一帧
        }

        canvasGroup.alpha = 0f; // 确保最终完全透明
        gameObject.SetActive(false); // 彻底隐藏血条物体，节省性能
    }
}
