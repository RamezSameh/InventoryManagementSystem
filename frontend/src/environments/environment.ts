// إعدادات البيئة الافتراضية (تُستخدم في builds الإنتاج).
// apiUrl الافتراضي يشير إلى `dotnet run` (http://localhost:5000)؛
// عدّلها لعنوان الـ API في بيئة الإنتاج عند النشر.
export const environment = {
  production: true,
  apiUrl: 'http://localhost:5000/api'
};
