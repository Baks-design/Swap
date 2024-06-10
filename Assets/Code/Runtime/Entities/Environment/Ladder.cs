using UnityEngine;

namespace SwapChains.Runtime.Entities.Environment
{
    public class Ladder : MonoBehaviour
    {
        [Header("Points to move to when reaching one of the extremities and moving off of the ladder")]
        [SerializeField] Transform bottomReleasePoint;
        [SerializeField] Transform topReleasePoint;

        [Header("Ladder segment")]
        [SerializeField] Vector3 LadderSegmentBottom;
        [SerializeField] float LadderSegmentLength;

        public Transform BottomReleasePoint => bottomReleasePoint;
        public Transform TopReleasePoint => topReleasePoint;
        // Gets the position of the bottom point of the ladder segment
        public Vector3 BottomAnchorPoint => transform.position + transform.TransformVector(LadderSegmentBottom);
        // Gets the position of the top point of the ladder segment
        public Vector3 TopAnchorPoint => transform.position + transform.TransformVector(LadderSegmentBottom) + (transform.up * LadderSegmentLength);

        public Vector3 ClosestPointOnLadderSegment(Vector3 fromPoint, out float onSegmentState)
        {
            var segment = TopAnchorPoint - BottomAnchorPoint;
            var segmentPoint1ToPoint = fromPoint - BottomAnchorPoint;
            var pointProjectionLength = Vector3.Dot(segmentPoint1ToPoint, segment.normalized);

            // When higher than bottom point
            if (pointProjectionLength > 0)
            {
                // If we are not higher than top point
                if (pointProjectionLength <= segment.magnitude)
                {
                    onSegmentState = 0;
                    return BottomAnchorPoint + (segment.normalized * pointProjectionLength);
                }
                // If we are higher than top point
                else
                {
                    onSegmentState = pointProjectionLength - segment.magnitude;
                    return TopAnchorPoint;
                }
            }
            // When lower than bottom point
            else
            {
                onSegmentState = pointProjectionLength;
                return BottomAnchorPoint;
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(BottomAnchorPoint, TopAnchorPoint);
        }
    }
}