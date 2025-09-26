/**
 * Particle System - manages visual effects and particles
 */
class ParticleSystem {
    constructor() {
        this.particles = [];
        this.maxParticles = 1000;
    }

    /**
     * Create explosion particles at a location
     * @param {number} x - X coordinate
     * @param {number} y - Y coordinate
     * @param {string} color - Particle color
     * @param {number} count - Number of particles to create
     * @param {number} intensity - Explosion intensity (affects speed)
     */
    createExplosion(x, y, color, count = 10, intensity = 1) {
        for (let i = 0; i < count && this.particles.length < this.maxParticles; i++) {
            this.particles.push({
                x: x,
                y: y,
                vx: (Math.random() - 0.5) * 8 * intensity,
                vy: (Math.random() - 0.5) * 8 * intensity,
                color: color,
                life: 30 + Math.random() * 20,
                maxLife: 30 + Math.random() * 20,
                size: 2 + Math.random() * 3,
                alpha: 1
            });
        }
    }

    /**
     * Create trail particles
     * @param {number} x - X coordinate
     * @param {number} y - Y coordinate
     * @param {string} color - Particle color
     * @param {number} count - Number of particles to create
     */
    createTrail(x, y, color, count = 3) {
        for (let i = 0; i < count && this.particles.length < this.maxParticles; i++) {
            this.particles.push({
                x: x + (Math.random() - 0.5) * 10,
                y: y + (Math.random() - 0.5) * 10,
                vx: (Math.random() - 0.5) * 2,
                vy: (Math.random() - 0.5) * 2,
                color: color,
                life: 15 + Math.random() * 10,
                maxLife: 15 + Math.random() * 10,
                size: 1 + Math.random() * 2,
                alpha: 0.8
            });
        }
    }

    /**
     * Create healing particles
     * @param {number} x - X coordinate
     * @param {number} y - Y coordinate
     * @param {number} count - Number of particles to create
     */
    createHealing(x, y, count = 8) {
        for (let i = 0; i < count && this.particles.length < this.maxParticles; i++) {
            this.particles.push({
                x: x + (Math.random() - 0.5) * 20,
                y: y + (Math.random() - 0.5) * 20,
                vx: (Math.random() - 0.5) * 1,
                vy: -Math.random() * 3 - 1, // Float upward
                color: '#00ff00',
                life: 40 + Math.random() * 20,
                maxLife: 40 + Math.random() * 20,
                size: 2 + Math.random() * 2,
                alpha: 1
            });
        }
    }

    /**
     * Create damage particles
     * @param {number} x - X coordinate
     * @param {number} y - Y coordinate
     * @param {number} count - Number of particles to create
     */
    createDamage(x, y, count = 6) {
        for (let i = 0; i < count && this.particles.length < this.maxParticles; i++) {
            this.particles.push({
                x: x + (Math.random() - 0.5) * 15,
                y: y + (Math.random() - 0.5) * 15,
                vx: (Math.random() - 0.5) * 4,
                vy: (Math.random() - 0.5) * 4,
                color: '#ff0000',
                life: 20 + Math.random() * 15,
                maxLife: 20 + Math.random() * 15,
                size: 1 + Math.random() * 2,
                alpha: 1
            });
        }
    }

    /**
     * Update all particles
     * @param {number} deltaTime - Time elapsed since last update
     */
    update(deltaTime) {
        for (let i = this.particles.length - 1; i >= 0; i--) {
            const particle = this.particles[i];
            
            // Update position
            particle.x += particle.vx;
            particle.y += particle.vy;
            
            // Apply gravity to some particles
            if (particle.color !== '#00ff00') { // Not healing particles
                particle.vy += 0.1;
            }
            
            // Update life
            particle.life--;
            particle.alpha = particle.life / particle.maxLife;
            
            // Remove dead particles
            if (particle.life <= 0) {
                this.particles.splice(i, 1);
            }
        }
    }

    /**
     * Render all particles
     * @param {CanvasRenderingContext2D} ctx - Canvas context
     */
    render(ctx) {
        this.particles.forEach(particle => {
            ctx.globalAlpha = particle.alpha;
            ctx.fillStyle = particle.color;
            
            // Add glow effect for certain particles
            if (particle.color === '#00ff00' || particle.color === '#ffff00') {
                ctx.shadowColor = particle.color;
                ctx.shadowBlur = 3;
            }
            
            ctx.fillRect(particle.x, particle.y, particle.size, particle.size);
            
            // Reset shadow
            ctx.shadowBlur = 0;
        });
        
        ctx.globalAlpha = 1;
    }

    /**
     * Clear all particles
     */
    clear() {
        this.particles = [];
    }

    /**
     * Get particle count
     * @returns {number} - Number of active particles
     */
    getParticleCount() {
        return this.particles.length;
    }
}

export default ParticleSystem;