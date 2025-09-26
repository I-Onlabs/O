import GameObject from '../core/GameObject.js';

/**
 * AngryDog class - represents hostile cybernetic hounds
 */
class AngryDog extends GameObject {
    constructor(x, y) {
        super(x, y, 40, 40);
        
        // Angry Dog specific properties
        this.health = 2;
        this.maxHealth = 2;
        this.speed = 1.5 + Math.random() * 0.5;
        this.color = '#ff4757';
        this.lastAttack = 0;
        this.attackCooldown = 1000; // 1 second
        this.attackDamage = 15;
        
        // AI behavior
        this.target = null;
        this.aggressionLevel = Math.random(); // 0-1, affects behavior
        this.attackRange = 30;
        
        // Visual effects
        this.isAttacking = false;
        this.attackAnimation = 0;
    }

    /**
     * Update Angry Dog AI and behavior
     * @param {Object} player - Player object to chase
     * @param {Object} nibble - Nibble object (secondary target)
     * @param {number} deltaTime - Time elapsed since last update
     */
    update(player, nibble, deltaTime) {
        this.updateAI(player, nibble);
        this.updateAttack(deltaTime);
        super.update(deltaTime);
    }

    /**
     * Update AI behavior
     * @param {Object} player - Player object to chase
     * @param {Object} nibble - Nibble object (secondary target)
     */
    updateAI(player, nibble) {
        // Choose target based on aggression level and proximity
        const playerDistance = this.getDistanceTo(player);
        const nibbleDistance = this.getDistanceTo(nibble);
        
        // More aggressive dogs prioritize Nibble
        if (this.aggressionLevel > 0.7 && nibbleDistance < playerDistance * 1.5) {
            this.target = nibble;
        } else {
            this.target = player;
        }
        
        // Move towards target
        this.moveTowardsTarget();
    }

    /**
     * Move towards the current target
     */
    moveTowardsTarget() {
        if (!this.target) return;
        
        const dx = this.target.x - this.x;
        const dy = this.target.y - this.y;
        const distance = Math.sqrt(dx * dx + dy * dy);
        
        if (distance > 0) {
            // Normalize direction and apply speed
            this.vx = (dx / distance) * this.speed;
            this.vy = (dy / distance) * this.speed;
        }
    }

    /**
     * Update attack behavior
     * @param {number} deltaTime - Time elapsed since last update
     */
    updateAttack(deltaTime) {
        const now = Date.now();
        
        if (this.isAttacking) {
            this.attackAnimation += deltaTime;
            if (this.attackAnimation > 200) {
                this.isAttacking = false;
                this.attackAnimation = 0;
            }
        }
        
        // Check if we can attack
        if (this.target && now - this.lastAttack > this.attackCooldown) {
            const distance = this.getDistanceTo(this.target);
            if (distance <= this.attackRange) {
                this.attack(this.target);
            }
        }
    }

    /**
     * Attack a target
     * @param {GameObject} target - Target to attack
     */
    attack(target) {
        this.lastAttack = Date.now();
        this.isAttacking = true;
        this.attackAnimation = 0;
        
        // Deal damage to target
        if (target.takeDamage) {
            target.takeDamage(this.attackDamage);
        }
    }

    /**
     * Take damage and potentially die
     * @param {number} damage - Amount of damage to take
     * @returns {boolean} - True if the dog dies
     */
    takeDamage(damage) {
        super.takeDamage(damage);
        
        // Angry dogs get more aggressive when damaged
        if (this.health > 0) {
            this.speed *= 1.2;
            this.aggressionLevel = Math.min(1, this.aggressionLevel + 0.3);
        }
        
        return this.health <= 0;
    }

    /**
     * Check if the dog is off-screen and should be removed
     * @param {number} screenWidth - Width of the game screen
     * @param {number} screenHeight - Height of the game screen
     * @returns {boolean} - True if should be removed
     */
    shouldRemove(screenWidth, screenHeight) {
        return this.x < -50 || 
               this.x > screenWidth + 50 || 
               this.y < -50 || 
               this.y > screenHeight + 50;
    }

    /**
     * Render the Angry Dog with detailed graphics
     * @param {CanvasRenderingContext2D} ctx - Canvas context
     */
    render(ctx) {
        if (!this.isActive) return;
        
        // Draw dog body
        ctx.fillStyle = this.color;
        ctx.fillRect(this.x, this.y, this.width, this.height);
        
        // Draw angry face
        ctx.fillStyle = '#ffffff';
        ctx.fillRect(this.x + 5, this.y + 5, 10, 10); // Head
        
        // Angry eyes
        ctx.fillStyle = '#ff0000';
        ctx.fillRect(this.x + 8, this.y + 8, 2, 2);
        ctx.fillRect(this.x + 12, this.y + 8, 2, 2);
        
        // Snarling mouth
        ctx.fillStyle = '#000000';
        ctx.fillRect(this.x + 9, this.y + 12, 4, 1);
        
        // Attack indicator
        if (this.isAttacking) {
            this.renderAttackIndicator(ctx);
        }
        
        // Aggression indicator
        this.renderAggressionIndicator(ctx);
        
        // Health bar
        this.renderHealthBar(ctx);
    }

    /**
     * Render attack animation indicator
     * @param {CanvasRenderingContext2D} ctx - Canvas context
     */
    renderAttackIndicator(ctx) {
        const alpha = 1 - (this.attackAnimation / 200);
        ctx.globalAlpha = alpha;
        
        // Red flash effect
        ctx.fillStyle = '#ff0000';
        ctx.fillRect(this.x - 5, this.y - 5, this.width + 10, this.height + 10);
        
        ctx.globalAlpha = 1;
    }

    /**
     * Render aggression level indicator
     * @param {CanvasRenderingContext2D} ctx - Canvas context
     */
    renderAggressionIndicator(ctx) {
        const barWidth = this.width;
        const barHeight = 2;
        const barY = this.y - 6;
        
        // Background
        ctx.fillStyle = '#333333';
        ctx.fillRect(this.x, barY, barWidth, barHeight);
        
        // Aggression level
        ctx.fillStyle = this.aggressionLevel > 0.7 ? '#ff0000' : 
                       this.aggressionLevel > 0.4 ? '#ff8800' : '#ffff00';
        ctx.fillRect(this.x, barY, barWidth * this.aggressionLevel, barHeight);
    }

    /**
     * Render health bar above the dog
     * @param {CanvasRenderingContext2D} ctx - Canvas context
     */
    renderHealthBar(ctx) {
        const barWidth = this.width;
        const barHeight = 3;
        const barY = this.y - 12;
        
        // Background
        ctx.fillStyle = '#333333';
        ctx.fillRect(this.x, barY, barWidth, barHeight);
        
        // Health
        const healthPercent = this.health / this.maxHealth;
        ctx.fillStyle = healthPercent > 0.5 ? '#00ff00' : '#ff0000';
        ctx.fillRect(this.x, barY, barWidth * healthPercent, barHeight);
    }
}

export default AngryDog;