/**
 * Collision System - handles all collision detection and response
 */
class CollisionSystem {
    constructor() {
        this.collisionCallbacks = new Map();
    }

    /**
     * Register a collision callback for specific object types
     * @param {string} type1 - First object type
     * @param {string} type2 - Second object type
     * @param {Function} callback - Function to call when collision occurs
     */
    registerCollision(type1, type2, callback) {
        const key = this.getCollisionKey(type1, type2);
        this.collisionCallbacks.set(key, callback);
    }

    /**
     * Get collision key for two types
     * @param {string} type1 - First type
     * @param {string} type2 - Second type
     * @returns {string} - Collision key
     */
    getCollisionKey(type1, type2) {
        return type1 < type2 ? `${type1}-${type2}` : `${type2}-${type1}`;
    }

    /**
     * Check collisions between two arrays of objects
     * @param {Array} objects1 - First array of objects
     * @param {Array} objects2 - Second array of objects
     * @param {string} type1 - Type of first objects
     * @param {string} type2 - Type of second objects
     */
    checkCollisions(objects1, objects2, type1, type2) {
        const key = this.getCollisionKey(type1, type2);
        const callback = this.collisionCallbacks.get(key);
        
        if (!callback) return;

        for (let i = objects1.length - 1; i >= 0; i--) {
            const obj1 = objects1[i];
            if (!obj1.isActive) continue;

            for (let j = objects2.length - 1; j >= 0; j--) {
                const obj2 = objects2[j];
                if (!obj2.isActive) continue;

                if (this.checkCollision(obj1, obj2)) {
                    const result = callback(obj1, obj2, i, j);
                    if (result && result.removeObj1) {
                        objects1.splice(i, 1);
                    }
                    if (result && result.removeObj2) {
                        objects2.splice(j, 1);
                    }
                }
            }
        }
    }

    /**
     * Check collision between two objects
     * @param {Object} obj1 - First object
     * @param {Object} obj2 - Second object
     * @returns {boolean} - True if collision detected
     */
    checkCollision(obj1, obj2) {
        return obj1.x < obj2.x + obj2.width &&
               obj1.x + obj1.width > obj2.x &&
               obj1.y < obj2.y + obj2.height &&
               obj1.y + obj1.height > obj2.y;
    }

    /**
     * Check collisions between objects and screen boundaries
     * @param {Object} obj - Object to check
     * @param {number} screenWidth - Screen width
     * @param {number} screenHeight - Screen height
     * @returns {Object} - Collision info with screen edges
     */
    checkScreenCollision(obj, screenWidth, screenHeight) {
        const collision = {
            left: obj.x < 0,
            right: obj.x + obj.width > screenWidth,
            top: obj.y < 0,
            bottom: obj.y + obj.height > screenHeight,
            any: false
        };

        collision.any = collision.left || collision.right || collision.top || collision.bottom;
        return collision;
    }

    /**
     * Check if object is completely off-screen
     * @param {Object} obj - Object to check
     * @param {number} screenWidth - Screen width
     * @param {number} screenHeight - Screen height
     * @param {number} margin - Margin outside screen to consider "off-screen"
     * @returns {boolean} - True if object is off-screen
     */
    isOffScreen(obj, screenWidth, screenHeight, margin = 50) {
        return obj.x + obj.width < -margin ||
               obj.x > screenWidth + margin ||
               obj.y + obj.height < -margin ||
               obj.y > screenHeight + margin;
    }

    /**
     * Get distance between two objects
     * @param {Object} obj1 - First object
     * @param {Object} obj2 - Second object
     * @returns {number} - Distance between objects
     */
    getDistance(obj1, obj2) {
        const dx = (obj1.x + obj1.width/2) - (obj2.x + obj2.width/2);
        const dy = (obj1.y + obj1.height/2) - (obj2.y + obj2.height/2);
        return Math.sqrt(dx * dx + dy * dy);
    }

    /**
     * Check if point is inside object
     * @param {number} x - Point X coordinate
     * @param {number} y - Point Y coordinate
     * @param {Object} obj - Object to check against
     * @returns {boolean} - True if point is inside object
     */
    isPointInObject(x, y, obj) {
        return x >= obj.x && x <= obj.x + obj.width &&
               y >= obj.y && y <= obj.y + obj.height;
    }

    /**
     * Get collision normal (direction of collision)
     * @param {Object} obj1 - First object
     * @param {Object} obj2 - Second object
     * @returns {Object} - Normal vector {x, y}
     */
    getCollisionNormal(obj1, obj2) {
        const center1 = {
            x: obj1.x + obj1.width / 2,
            y: obj1.y + obj1.height / 2
        };
        const center2 = {
            x: obj2.x + obj2.width / 2,
            y: obj2.y + obj2.height / 2
        };

        const dx = center1.x - center2.x;
        const dy = center1.y - center2.y;
        const distance = Math.sqrt(dx * dx + dy * dy);

        if (distance === 0) {
            return { x: 0, y: 0 };
        }

        return {
            x: dx / distance,
            y: dy / distance
        };
    }
}

export default CollisionSystem;