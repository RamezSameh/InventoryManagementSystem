// إعدادات البيئة الافتراضية (تُستخدم في builds الإنتاج).
// apiUrl الافتراضي يشير إلى IIS Express (https://localhost:44330)؛
// عدّلها لعنوان الـ API في بيئة الإنتاج عند النشر،
// ولو شغّلت الـ API بـ `dotnet run` غيّرها إلى http://localhost:5000/api.
export const environment = {
  production: true,
  apiUrl: 'https://localhost:44330/api'
};
