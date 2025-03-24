using XablabAutoPost.Core.Saver;

namespace XablabAutoPost.Framework.SettingsSaver;


[Serializable]
public class MessageTemplate
{
    public MessageTemplate(string message)
    {
        Message = message;
    }

    public string Message { get; set; }
}


public class VkMessagesData
{
    public VkMessagesData(List<MessageTemplate> messageDataTemplate)
    {
        MessageDataTemplate = messageDataTemplate;
    }

    public List<MessageTemplate> MessageDataTemplate { get; set; }
}

public class VkMessagesTemplatesSaver : Saver<VkMessagesData>
{
    protected override string DirectoryName => "VkMessagesData";
    protected override string FileName => "VkMessagesData";
    
    public VkMessagesData LoadMessagesTemplatesData()
    {
        var data = Load()
                   ?? new VkMessagesData(new List<MessageTemplate>()
                   {
                       new MessageTemplate("\ud83d\udd25 Новая модель: {0}!\n\ud83c\udf81 Скидка {1}% по промокоду: {2}\n\ud83d\ude80 Не пропусти выгодное предложение! Узнай стоимость: xablab.ru/upload\n\u2757\ufe0f Чтобы зафиксировать скидку:\n 1 Оформи заказ в течение 2 часов.\n 2 Укажи промокод в комментариях к заказу.\n\u26a0\ufe0f Если условия не соблюдены, скидка не действует.\n#ХабЛаб #3D_печать\nP.S. Через 2 часа скидка изменится! Успей поймать свою выгоду!"),
                       new MessageTemplate("\u2728 Внимание! Новая модель: {0}!\n\ud83d\udcb0 Скидка {1}% по промокоду: {2}\n\ud83d\udd17 Не пропусти выгодное предложение! Переходи: xablab.ru/upload\n\u2757\ufe0f Чтобы зафиксировать скидку:\n 1 Оформи заказ в течение 2 часов.\n 2 Укажи промокод в комментариях к заказу.\n\u26a0\ufe0f Если условия не соблюдены, скидка не действует.\n#ХабЛаб #3D_печать\nP.S. Через 2 часа скидка может быть другой! Следи за обновлениями!"),
                       new MessageTemplate("\ud83c\udf89 {0} — твой новый проект!\n\ud83c\udf81 Скидка {1}% по промокоду: {2}\n\ud83d\ude80 Не пропусти выгодное предложение! Узнай цену: xablab.ru/upload\n\u2757\ufe0f Чтобы зафиксировать скидку:\n 1 Оформи заказ в течение 2 часов.\n 2 Укажи промокод в комментариях к заказу.\n\u26a0\ufe0f Если условия не соблюдены, скидка не действует.\n#ХабЛаб #3D_печать\nP.S. Через 2 часа скидка изменится! Лови момент!"),
                       new MessageTemplate("\ud83d\ude81 Новая модель уже здесь: {0}!\n\ud83d\udd25 Скидка {1}% по промокоду: {2}\n\ud83d\udca5 Не пропусти выгодное предложение! Забирай модель: xablab.ru/upload\n\u2757\ufe0f Чтобы зафиксировать скидку:\n 1 Оформи заказ в течение 2 часов.\n 2 Укажи промокод в комментариях к заказу.\n\u26a0\ufe0f Если условия не соблюдены, скидка не действует.\n#ХабЛаб #3D_печать\nP.S. Через 2 часа скидка станет другой! Успей!"),
                       new MessageTemplate("\ud83c\udf1f {0} — новинка, которая тебя удивит!\n\ud83c\udf81 Скидка {1}% по промокоду: {2}\n\ud83d\udd17 Не пропусти выгодное предложение! Скачивай: xablab.ru/upload\n\u2757\ufe0f Чтобы зафиксировать скидку:\n 1 Оформи заказ в течение 2 часов.\n 2 Укажи промокод в комментариях к заказу.\n\u26a0\ufe0f Если условия не соблюдены, скидка не действует.\n#ХабЛаб #3D_печать\nP.S. Через 2 часа скидка изменится! Будь на шаг впереди!"),
                   });

        Save(data);

        return data;
    }
}