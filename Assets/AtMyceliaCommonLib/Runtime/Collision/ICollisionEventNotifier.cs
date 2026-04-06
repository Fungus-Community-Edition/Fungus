using UnityEngine;
using UnityEngine.Events;

namespace AtMycelia.Events
{
    public interface ICollisionEventNotifier 
    {
        #region TwoDim
        event UnityAction<Collider2D> TriggerEnter2D;
        event UnityAction<Collider2D> TriggerExit2D;
        event UnityAction<Collider2D> TriggerStay2D;

        event UnityAction<Collision2D> CollisionEnter2D;
        event UnityAction<Collision2D> CollisionExit2D;
        event UnityAction<Collision2D> CollisionStay2D;
        #endregion

        #region ThreeDim
        event UnityAction<Collider> TriggerEnter;
        event UnityAction<Collider> TriggerExit;
        event UnityAction<Collider> TriggerStay;

        event UnityAction<Collision> CollisionEnter;
        event UnityAction<Collision> CollisionExit;
        event UnityAction<Collision> CollisionStay;
        #endregion

        event UnityAction<GameObject> ParticleCollision;

    }
}