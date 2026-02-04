using System;

public class TreeHealth
{
    private readonly double _maxHealth;
    private double _currentHealth;

    public double MaxHealth => _maxHealth;
    public double CurrentHealth => _currentHealth;
    public double HealthPercent => (_currentHealth / _maxHealth) * 100.0;
    public bool IsDead => _currentHealth <= 0;

    public event Action<double> OnHealthChanged;
    public event Action OnDeath;

    public TreeHealth(double maxHealth = 100.0)
    {
        if (maxHealth <= 0)
        {
            throw new ArgumentException("최대 체력은 0보다 커야 합니다.");
        }

        _maxHealth = maxHealth;
        _currentHealth = maxHealth;
    }

    // 데미지를 받습니다
    public void TakeDamage(double damage)
    {
        if (damage < 0)
        {
            throw new ArgumentException("데미지는 0 이상이어야 합니다.");
        }

        _currentHealth = Math.Max(0, _currentHealth - damage);
        OnHealthChanged?.Invoke(_currentHealth);

        if (IsDead)
        {
            OnDeath?.Invoke();
        }
    }

    // 체력을 회복합니다 (리스폰)
    public void Heal(double amount)
    {
        _currentHealth = Math.Min(_maxHealth, _currentHealth + amount);
        OnHealthChanged?.Invoke(_currentHealth);
    }

    // 전체 체력 회복 (리스폰)
    public void Reset()
    {
        _currentHealth = _maxHealth;
        OnHealthChanged?.Invoke(_currentHealth);
    }
}