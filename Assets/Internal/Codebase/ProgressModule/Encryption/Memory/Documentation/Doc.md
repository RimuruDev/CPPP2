# Документация по шифрованию данных в Unity

## Введение

Этот проект содержит два класса для шифрования данных: **XOR** и **AES**. Они предназначены для защиты данных в памяти, таких как личная информация пользователя или данные, которые требуют высокого уровня безопасности.

### Когда использовать:
1. **AES**: Используется для шифрования важной информации, такой как сессионные ключи, персональные данные, или любые другие данные, которые должны быть защищены от утечек и манипуляций. AES обеспечивает высокий уровень безопасности.
2. **XOR**: Используется для быстрого шифрования данных, которые не столь чувствительны, но должны быть защищены от случайных изменений. Применяется для данных, которые часто меняются, например, статистики, баланса и т.д.

### Не используйте:
- XOR для чувствительных данных, таких как пароли или финансовая информация, так как XOR легко поддается атаке при определенных условиях.
- Не используйте эти алгоритмы для длительного хранения данных. Они предназначены для защиты данных в памяти во время работы игры.

---

## Классы и методы

### 1. **XorEncryptionService**
Класс для шифрования и дешифрования данных с использованием алгоритма XOR.

#### Примитивы:
- **EncryptIntWithXor(int value)**: Шифрует целочисленное значение с помощью XOR.
- **DecryptIntWithXor(byte[] encryptedValue)**: Дешифрует целочисленное значение.
- **EncryptFloatWithXor(float value)**: Шифрует значение с плавающей точкой с помощью XOR.
- **DecryptFloatWithXor(byte[] encryptedValue)**: Дешифрует значение с плавающей точкой.
- **EncryptBoolWithXor(bool value)**: Шифрует булевое значение с помощью XOR.
- **DecryptBoolWithXor(byte[] encryptedValue)**: Дешифрует булевое значение.

#### Коллекции:
- **EncryptListWithXor(List<int> list)**: Шифрует список целых чисел.
- **DecryptListWithXor(List<byte[]> encryptedList)**: Дешифрует список зашифрованных целых чисел.

#### Строки:
- **EncryptWithXor(string value)**: Шифрует строку с использованием XOR и Base64.
- **DecryptWithXor(string encryptedValue)**: Дешифрует строку с использованием XOR.

#### Объекты:
- **SerializeObject(object obj)**: Сериализует объект в байтовый массив.
- **DeserializeObject(byte[] data)**: Восстанавливает объект из байтового массива.
- **EncryptObjectWithXor(object obj)**: Шифрует объект с использованием XOR.
- **DecryptObjectWithXor(byte[] encryptedData)**: Дешифрует объект с использованием XOR.

---

### 2. **AesEncryptionService**
Класс для шифрования и дешифрования данных с использованием алгоритма AES. AES рекомендуется для более чувствительных данных.

#### Инициализация сессионного ключа:
- **GenerateSessionKey()**: Генерирует новый сессионный ключ и вектор инициализации (IV). Этот метод вызывается автоматически, но также можно вызвать его вручную, если требуется перегенерация ключа.

#### Примитивы:
- **EncryptIntWithAes(int value)**: Шифрует целочисленное значение с помощью AES.
- **DecryptIntWithAes(byte[] encryptedValue)**: Дешифрует целочисленное значение.
- **EncryptFloatWithAes(float value)**: Шифрует значение с плавающей точкой с помощью AES.
- **DecryptFloatWithAes(byte[] encryptedValue)**: Дешифрует значение с плавающей точкой.
- **EncryptBoolWithAes(bool value)**: Шифрует булевое значение с помощью AES.
- **DecryptBoolWithAes(byte[] encryptedValue)**: Дешифрует булевое значение.

#### Коллекции:
- **EncryptListWithAes(List<int> list)**: Шифрует список целых чисел.
- **DecryptListWithAes(List<byte[]> encryptedList)**: Дешифрует список целых чисел.

#### Строки:
- **EncryptStringWithAes(string value)**: Шифрует строку с помощью AES.
- **DecryptStringWithAes(string encryptedValue)**: Дешифрует строку с использованием AES.

#### Объекты:
- **SerializeObject(object obj)**: Сериализует объект в байтовый массив с использованием **BinaryFormatter**.
- **DeserializeObject(byte[] data)**: Восстанавливает объект из байтового массива.
- **EncryptObjectWithAes(object obj)**: Шифрует объект с использованием AES.
- **DecryptObjectWithAes(byte[] encryptedData)**: Дешифрует объект с использованием AES.

---

## Пример использования

### Пример для **AES** шифрования:

```csharp
// Инициализация данных
int originalInt = 123456;
string originalString = "Test String";

// Шифрование
byte[] encryptedInt = AesEncryptionService.EncryptIntWithAes(originalInt);
string encryptedString = AesEncryptionService.EncryptStringWithAes(originalString);

// Дешифрование
int decryptedInt = AesEncryptionService.DecryptIntWithAes(encryptedInt);
string decryptedString = AesEncryptionService.DecryptStringWithAes(encryptedString);

// Результат
UnityEngine.Debug.Log($"Decrypted Int: {decryptedInt}, Decrypted String: {decryptedString}");
```

### Пример для **XOR** шифрования:

```csharp
// Инициализация данных
int originalInt = 123456;
string originalString = "Test String";

// Шифрование
byte[] encryptedInt = XorEncryptionService.EncryptIntWithXor(originalInt);
string encryptedString = XorEncryptionService.EncryptWithXor(originalString);

// Дешифрование
int decryptedInt = XorEncryptionService.DecryptIntWithXor(encryptedInt);
string decryptedString = XorEncryptionService.DecryptWithXor(encryptedString);

// Результат
UnityEngine.Debug.Log($"Decrypted Int: {decryptedInt}, Decrypted String: {decryptedString}");
```

---

## Советы по использованию

1. **Использование AES**: Рекомендуется для всех важных и конфиденциальных данных. Это идеальный выбор для защиты личной информации, паролей, сессионных данных и т. д.
2. **Использование XOR**: Быстрое шифрование, но менее безопасное, чем AES. Применяйте XOR для менее важных данных или для данных, которые часто изменяются (например, статистика или временные данные).
3. **Сессионный ключ**: Каждый раз, когда вы хотите сменить ключ для AES, используйте метод `GenerateSessionKey()`. Это поможет обновить ключ и IV для защиты данных.
4. **Не храните зашифрованные данные слишком долго в памяти**: Даже зашифрованные данные в оперативной памяти могут быть уязвимы для атак, поэтому старайтесь минимизировать время их хранения.
