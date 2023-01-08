using Godot;
using Godot.Collections;

namespace ldjam52.Game.Utils;

public static class PhysicsDirectSpaceState2DExtension
{
    public static RaycastHit? IntersectRayEnhanced(this PhysicsDirectSpaceState2D state, PhysicsRayQueryParameters2D parameters)
    {
        var dictionary = state.IntersectRay(parameters);
        if (dictionary.Count == 0)
        {
            return null;
        }

        return RaycastHit.Of(dictionary);
    }
    
    public static ShapecastHit[] IntersectShapeEnhanced(this PhysicsDirectSpaceState2D state, PhysicsShapeQueryParameters2D parameters, int maxResults = 32)
    {
        var dictionaries = state.IntersectShape(parameters, maxResults);
        var raycastHits = new ShapecastHit[dictionaries.Count];
        for (var i = 0; i < dictionaries.Count; i++)
        {
            raycastHits[i] = ShapecastHit.Of(dictionaries[i]);
        }

        return raycastHits;
    }

    public static RaycastHit? IntersectRay(this PhysicsDirectSpaceState2D state, Vector2 origin, Vector2 direction, float maxLength)
    {
        var parameters = new PhysicsRayQueryParameters2D();
        parameters.From = origin;
        parameters.To = origin + direction * maxLength;
        return IntersectRayEnhanced(state, parameters);
    }
}

public struct RaycastHit
{
    public Vector2 Position;
    public Vector2 Normal;

    public static RaycastHit Of(Dictionary dictionary)
    {
        // collider: The colliding object.  
        // collider_id: The colliding object's ID.  
        // normal: The object's surface normal at the intersection point, or Vector2(0, 0) if the ray starts inside the shape and HitFromInside is true.  
        // position: The intersection point.  
        // rid: The intersecting object's RID.  
        // shape: The shape index of the colliding shape.
        return new RaycastHit
        {
            Position = dictionary["position"].AsVector2(),
            Normal = dictionary["normal"].AsVector2(),
        };
    }
}

public struct ShapecastHit
{
    public Variant Collider;

    public static ShapecastHit Of(Dictionary dictionary)
    {
        // collider: The colliding object.
        // collider_id: The colliding object's ID.
        // rid: The intersecting object's RID.
        // shape: The shape index of the colliding shape.
        return new ShapecastHit
        {
            Collider = dictionary["collider"],
        };
    }
}
