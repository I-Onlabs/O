import GameObject from '../core/GameObject.js';

/**
 * Obstacle class - represents various cyberpunk obstacles
 * Implements all 7 obstacle types from the Game Design Document
 */
class Obstacle extends GameObject {
    constructor(type, x, y) {
        super(x, y, 60, 60);
        
        this.type = type;
        this.health = this.getHealthForType(type);
        this.maxHealth = this.health;
        this.color = this.getColorForType(type);
        this.effect = null;
        this.animationTime = 0;
        this.bounceHeight = 0;
        this.rotation = 0;
        this.pulseScale = 1;
        this.slimeTrail = [];
        this.fleaCount = 0;
        this.leashLength = 0;
        this.cannonCooldown = 0;
        this.toyBounce = 0;
        
        // Initialize type-specific properties
        this.initializeTypeProperties();
    }

    /**
     * Initialize properties specific to obstacle type
     */
    initializeTypeProperties() {
        switch (this.type) {
            case 'bouncyBoneDrone':
                this.bounceHeight = Math.random() * 20 + 10;
                this.vy = -2 - Math.random() * 2;
                break;
            case 'slobberMine':
                this.slimeTrail = [];
                this.pulseScale = 0.8;
                break;
            case 'holoFleaSwarm':
                this.fleaCount = 8 + Math.floor(Math.random() * 5);
                this.health = 1; // Swarm is destroyed in one hit
                this.maxHealth = 1;
                break;
            case 'runawayDogWalker':
                this.leashLength = 40;
                this.rotation = Math.random() * Math.PI * 2;
                break;
            case 'kibbleCannon':
                this.cannonCooldown = 0;
                this.health = 2;
                this.maxHealth = 2;
                break;
            case 'neonChewToy':
                this.toyBounce = Math.random() * Math.PI * 2;
                this.health = 1;
                this.maxHealth = 1;
                break;
        }
    }

    /**
     * Get health value for obstacle type
     * @param {string} type - Obstacle type
     * @returns {number} - Health value
     */
    getHealthForType(type) {
        const healthMap = {
            'bouncyBoneDrone': 3,
            'slobberMine': 2,
            'holoFleaSwarm': 1,
            'runawayDogWalker': 2,
            'kibbleCannon': 2,
            'neonChewToy': 1
        };
        return healthMap[type] || 1;
    }

    /**
     * Get color for obstacle type
     * @param {string} type - Obstacle type
     * @returns {string} - Color hex code
     */
    getColorForType(type) {
        const colorMap = {
            'bouncyBoneDrone': '#ff6b6b',
            'slobberMine': '#4ecdc4',
            'holoFleaSwarm': '#45b7d1',
            'runawayDogWalker': '#96ceb4',
            'kibbleCannon': '#feca57',
            'neonChewToy': '#ff9ff3'
        };
        return colorMap[type] || '#ffffff';
    }

    /**
     * Update obstacle behavior
     * @param {number} deltaTime - Time elapsed since last update
     */
    update(deltaTime) {
        this.animationTime += deltaTime;
        
        // Update type-specific behavior
        switch (this.type) {
            case 'bouncyBoneDrone':
                this.updateBouncyBoneDrone(deltaTime);
                break;
            case 'slobberMine':
                this.updateSlobberMine(deltaTime);
                break;
            case 'holoFleaSwarm':
                this.updateHoloFleaSwarm(deltaTime);
                break;
            case 'runawayDogWalker':
                this.updateRunawayDogWalker(deltaTime);
                break;
            case 'kibbleCannon':
                this.updateKibbleCannon(deltaTime);
                break;
            case 'neonChewToy':
                this.updateNeonChewToy(deltaTime);
                break;
        }
        
        super.update(deltaTime);
    }

    /**
     * Update bouncy bone drone behavior
     * @param {number} deltaTime - Time elapsed since last update
     */
    updateBouncyBoneDrone(deltaTime) {
        // Bouncing motion
        this.y += this.vy;
        this.vy += 0.1; // Gravity
        
        // Bounce when hitting ground
        if (this.y > this.bounceHeight) {
            this.y = this.bounceHeight;
            this.vy = -Math.abs(this.vy) * 0.8;
        }
        
        // Side-to-side movement
        this.x += Math.sin(this.animationTime * 0.01) * 0.5;
    }

    /**
     * Update slobber mine behavior
     * @param {number} deltaTime - Time elapsed since last update
     */
    updateSlobberMine(deltaTime) {
        // Pulsing effect
        this.pulseScale = 0.8 + Math.sin(this.animationTime * 0.005) * 0.2;
        
        // Create slime trail
        if (Math.random() < 0.1) {
            this.slimeTrail.push({
                x: this.x + Math.random() * this.width,
                y: this.y + this.height,
                life: 60,
                maxLife: 60
            });
        }
        
        // Update slime trail
        this.slimeTrail = this.slimeTrail.filter(drop => {
            drop.life--;
            return drop.life > 0;
        });
    }

    /**
     * Update holo flea swarm behavior
     * @param {number} deltaTime - Time elapsed since last update
     */
    updateHoloFleaSwarm(deltaTime) {
        // Swarm movement - chaotic
        this.x += (Math.random() - 0.5) * 2;
        this.y += (Math.random() - 0.5) * 2;
        
        // Shake effect
        this.rotation = Math.sin(this.animationTime * 0.02) * 0.1;
    }

    /**
     * Update runaway dog walker behavior
     * @param {number} deltaTime - Time elapsed since last update
     */
    updateRunawayDogWalker(deltaTime) {
        // Erratic movement
        this.rotation += (Math.random() - 0.5) * 0.1;
        this.x += Math.cos(this.rotation) * 0.5;
        this.y += Math.sin(this.rotation) * 0.3;
        
        // Leash whipping
        this.leashLength = 30 + Math.sin(this.animationTime * 0.01) * 10;
    }

    /**
     * Update kibble cannon behavior
     * @param {number} deltaTime - Time elapsed since last update
     */
    updateKibbleCannon(deltaTime) {
        // Cannon firing
        this.cannonCooldown--;
        if (this.cannonCooldown <= 0) {
            this.cannonCooldown = 60 + Math.random() * 30; // Fire every 1-1.5 seconds
            this.fireKibble();
        }
        
        // Cannon rotation
        this.rotation = Math.sin(this.animationTime * 0.005) * 0.2;
    }

    /**
     * Update neon chew toy behavior
     * @param {number} deltaTime - Time elapsed since last update
     */
    updateNeonChewToy(deltaTime) {
        // Bouncing motion
        this.toyBounce += 0.1;
        this.y += Math.sin(this.toyBounce) * 0.5;
        
        // Random direction changes
        if (Math.random() < 0.02) {
            this.vx = (Math.random() - 0.5) * 2;
            this.vy = (Math.random() - 0.5) * 2;
        }
    }

    /**
     * Fire kibble from cannon
     */
    fireKibble() {
        // This would create kibble projectiles
        // For now, just create a visual effect
        this.effect = {
            type: 'kibbleShot',
            x: this.x + this.width,
            y: this.y + this.height / 2,
            life: 30
        };
    }

    /**
     * Take damage and handle destruction effects
     * @param {number} damage - Amount of damage to take
     * @returns {boolean} - True if obstacle is destroyed
     */
    takeDamage(damage) {
        super.takeDamage(damage);
        
        if (this.health <= 0) {
            this.createDestructionEffect();
            return true;
        }
        
        return false;
    }

    /**
     * Create destruction effect based on obstacle type
     */
    createDestructionEffect() {
        switch (this.type) {
            case 'bouncyBoneDrone':
                this.effect = { type: 'holoTreats', count: 5 };
                break;
            case 'slobberMine':
                this.effect = { type: 'slobberSlick', radius: 50 };
                break;
            case 'holoFleaSwarm':
                this.effect = { type: 'sparkSprinkles', count: 8 };
                break;
            case 'runawayDogWalker':
                this.effect = { type: 'shockNet', length: 60 };
                break;
            case 'kibbleCannon':
                this.effect = { type: 'kibbleExplosion', count: 10 };
                break;
            case 'neonChewToy':
                this.effect = { type: 'squeakyStorm', radius: 40 };
                break;
        }
    }

    /**
     * Render the obstacle with type-specific graphics
     * @param {CanvasRenderingContext2D} ctx - Canvas context
     */
    render(ctx) {
        if (!this.isActive) return;
        
        ctx.save();
        
        // Apply transformations
        ctx.translate(this.x + this.width / 2, this.y + this.height / 2);
        ctx.scale(this.pulseScale, this.pulseScale);
        ctx.rotate(this.rotation);
        
        // Render based on type
        switch (this.type) {
            case 'bouncyBoneDrone':
                this.renderBouncyBoneDrone(ctx);
                break;
            case 'slobberMine':
                this.renderSlobberMine(ctx);
                break;
            case 'holoFleaSwarm':
                this.renderHoloFleaSwarm(ctx);
                break;
            case 'runawayDogWalker':
                this.renderRunawayDogWalker(ctx);
                break;
            case 'kibbleCannon':
                this.renderKibbleCannon(ctx);
                break;
            case 'neonChewToy':
                this.renderNeonChewToy(ctx);
                break;
        }
        
        ctx.restore();
        
        // Render effects
        this.renderEffects(ctx);
    }

    /**
     * Render bouncy bone drone
     * @param {CanvasRenderingContext2D} ctx - Canvas context
     */
    renderBouncyBoneDrone(ctx) {
        // Drone body
        ctx.fillStyle = this.color;
        ctx.fillRect(-this.width / 2, -this.height / 2, this.width, this.height);
        
        // Bone
        ctx.fillStyle = '#ffffff';
        ctx.fillRect(-10, -5, 20, 10);
        
        // Propellers
        ctx.strokeStyle = '#cccccc';
        ctx.lineWidth = 2;
        ctx.beginPath();
        ctx.moveTo(-15, -15);
        ctx.lineTo(15, 15);
        ctx.moveTo(15, -15);
        ctx.lineTo(-15, 15);
        ctx.stroke();
    }

    /**
     * Render slobber mine
     * @param {CanvasRenderingContext2D} ctx - Canvas context
     */
    renderSlobberMine(ctx) {
        // Mine body
        ctx.fillStyle = this.color;
        ctx.fillRect(-this.width / 2, -this.height / 2, this.width, this.height);
        
        // Slime drops
        ctx.fillStyle = '#4ecdc4';
        for (let i = 0; i < 3; i++) {
            const angle = (i * Math.PI * 2) / 3;
            const x = Math.cos(angle) * 15;
            const y = Math.sin(angle) * 15;
            ctx.fillRect(x - 3, y - 3, 6, 6);
        }
    }

    /**
     * Render holo flea swarm
     * @param {CanvasRenderingContext2D} ctx - Canvas context
     */
    renderHoloFleaSwarm(ctx) {
        // Draw individual fleas
        ctx.fillStyle = this.color;
        for (let i = 0; i < this.fleaCount; i++) {
            const angle = (i * Math.PI * 2) / this.fleaCount;
            const radius = 15 + Math.sin(this.animationTime * 0.01 + i) * 5;
            const x = Math.cos(angle) * radius;
            const y = Math.sin(angle) * radius;
            ctx.fillRect(x - 2, y - 2, 4, 4);
        }
    }

    /**
     * Render runaway dog walker
     * @param {CanvasRenderingContext2D} ctx - Canvas context
     */
    renderRunawayDogWalker(ctx) {
        // Walker body
        ctx.fillStyle = this.color;
        ctx.fillRect(-this.width / 2, -this.height / 2, this.width, this.height);
        
        // Leash
        ctx.strokeStyle = '#ffff00';
        ctx.lineWidth = 3;
        ctx.beginPath();
        ctx.moveTo(this.width / 2, 0);
        ctx.lineTo(this.width / 2 + this.leashLength, -10);
        ctx.stroke();
        
        // Electric effect
        ctx.strokeStyle = '#00ffff';
        ctx.lineWidth = 1;
        for (let i = 0; i < 3; i++) {
            ctx.beginPath();
            ctx.moveTo(this.width / 2 + i * 5, 0);
            ctx.lineTo(this.width / 2 + this.leashLength + (Math.random() - 0.5) * 10, -10);
            ctx.stroke();
        }
    }

    /**
     * Render kibble cannon
     * @param {CanvasRenderingContext2D} ctx - Canvas context
     */
    renderKibbleCannon(ctx) {
        // Cannon base
        ctx.fillStyle = this.color;
        ctx.fillRect(-this.width / 2, -this.height / 2, this.width, this.height);
        
        // Cannon barrel
        ctx.fillStyle = '#333333';
        ctx.fillRect(this.width / 2 - 5, -5, 15, 10);
        
        // Muzzle flash
        if (this.cannonCooldown > 50) {
            ctx.fillStyle = '#ffff00';
            ctx.fillRect(this.width / 2 + 10, -3, 8, 6);
        }
    }

    /**
     * Render neon chew toy
     * @param {CanvasRenderingContext2D} ctx - Canvas context
     */
    renderNeonChewToy(ctx) {
        // Toy body
        ctx.fillStyle = this.color;
        ctx.fillRect(-this.width / 2, -this.height / 2, this.width, this.height);
        
        // Neon glow effect
        ctx.shadowColor = this.color;
        ctx.shadowBlur = 10;
        ctx.fillRect(-this.width / 2, -this.height / 2, this.width, this.height);
        ctx.shadowBlur = 0;
        
        // Bite marks
        ctx.fillStyle = '#000000';
        ctx.fillRect(-5, -5, 3, 3);
        ctx.fillRect(2, 2, 3, 3);
    }

    /**
     * Render special effects
     * @param {CanvasRenderingContext2D} ctx - Canvas context
     */
    renderEffects(ctx) {
        if (!this.effect) return;
        
        switch (this.effect.type) {
            case 'holoTreats':
                this.renderHoloTreats(ctx);
                break;
            case 'slobberSlick':
                this.renderSlobberSlick(ctx);
                break;
            case 'sparkSprinkles':
                this.renderSparkSprinkles(ctx);
                break;
            case 'shockNet':
                this.renderShockNet(ctx);
                break;
            case 'kibbleExplosion':
                this.renderKibbleExplosion(ctx);
                break;
            case 'squeakyStorm':
                this.renderSqueakyStorm(ctx);
                break;
        }
    }

    /**
     * Render holo treats effect
     * @param {CanvasRenderingContext2D} ctx - Canvas context
     */
    renderHoloTreats(ctx) {
        ctx.fillStyle = '#ffff00';
        for (let i = 0; i < this.effect.count; i++) {
            const angle = (i * Math.PI * 2) / this.effect.count;
            const x = this.x + Math.cos(angle) * 30;
            const y = this.y + Math.sin(angle) * 30;
            ctx.fillRect(x, y, 4, 4);
        }
    }

    /**
     * Render slobber slick effect
     * @param {CanvasRenderingContext2D} ctx - Canvas context
     */
    renderSlobberSlick(ctx) {
        ctx.fillStyle = 'rgba(78, 205, 196, 0.3)';
        ctx.beginPath();
        ctx.arc(this.x + this.width / 2, this.y + this.height / 2, this.effect.radius, 0, Math.PI * 2);
        ctx.fill();
    }

    /**
     * Render spark sprinkles effect
     * @param {CanvasRenderingContext2D} ctx - Canvas context
     */
    renderSparkSprinkles(ctx) {
        ctx.fillStyle = '#00ffff';
        for (let i = 0; i < this.effect.count; i++) {
            const x = this.x + Math.random() * this.width;
            const y = this.y + Math.random() * this.height;
            ctx.fillRect(x, y, 2, 2);
        }
    }

    /**
     * Render shock net effect
     * @param {CanvasRenderingContext2D} ctx - Canvas context
     */
    renderShockNet(ctx) {
        ctx.strokeStyle = '#ffff00';
        ctx.lineWidth = 2;
        ctx.beginPath();
        ctx.moveTo(this.x, this.y);
        ctx.lineTo(this.x + this.effect.length, this.y - 20);
        ctx.lineTo(this.x + this.effect.length * 2, this.y);
        ctx.stroke();
    }

    /**
     * Render kibble explosion effect
     * @param {CanvasRenderingContext2D} ctx - Canvas context
     */
    renderKibbleExplosion(ctx) {
        ctx.fillStyle = '#feca57';
        for (let i = 0; i < this.effect.count; i++) {
            const angle = Math.random() * Math.PI * 2;
            const distance = Math.random() * 40;
            const x = this.x + Math.cos(angle) * distance;
            const y = this.y + Math.sin(angle) * distance;
            ctx.fillRect(x, y, 3, 3);
        }
    }

    /**
     * Render squeaky storm effect
     * @param {CanvasRenderingContext2D} ctx - Canvas context
     */
    renderSqueakyStorm(ctx) {
        ctx.fillStyle = 'rgba(255, 159, 243, 0.5)';
        ctx.beginPath();
        ctx.arc(this.x + this.width / 2, this.y + this.height / 2, this.effect.radius, 0, Math.PI * 2);
        ctx.fill();
    }
}

export default Obstacle;