using UnityEngine;

namespace CrystalRuins.Presentation
{
    public sealed class EntityView : MonoBehaviour
    {
        const float Speed = 14f;

        Vector3 target;

        public void SnapTo(Vector2Int cell)
        {
            target = new Vector3(cell.x, cell.y, 0);
            transform.localPosition = target;
        }

        public void MoveTo(Vector2Int cell) => target = new Vector3(cell.x, cell.y, 0);

        void Update()
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, target, Speed * Time.deltaTime);
        }
    }
}
