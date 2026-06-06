using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SmartLightTrigger : MonoBehaviour
{
    [Tooltip("Поставь галочку, если темнота начинается СПРАВА от триггера")]
    public bool isCaveToTheRight = true;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"[Trigger] ВОШЁЛ в зону: {collision.name}, тег Player={collision.CompareTag("Player")}");
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Debug.Log($"[Trigger] ВЫШЕЛ из зоны: {collision.name}");

        if (!collision.CompareTag("Player"))
        {
            Debug.Log("[Trigger] СТОП: это не Player (проверь тег игрока)");
            return;
        }

        if (CaveLightController.Instance == null)
        {
            Debug.Log("[Trigger] СТОП: CaveLightController.Instance == null (нет контроллера в сцене или не проснулся)");
            return;
        }

        bool playerIsRight = collision.transform.position.x > transform.position.x;
        bool isInsideCave = isCaveToTheRight ? playerIsRight : !playerIsRight;

        Debug.Log($"[Trigger] игрокСправа={playerIsRight}, пещераСправа={isCaveToTheRight}, => входВпещеру={isInsideCave}");

        if (isInsideCave)
        {
            Debug.Log("[Trigger] -> EnterDark()");
            CaveLightController.Instance.EnterDark();
        }
        else
        {
            Debug.Log("[Trigger] -> ExitToLight()");
            CaveLightController.Instance.ExitToLight();
        }
    }
}