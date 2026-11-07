// Preview only: never presented as a persisted database document.
export const sample = {
  book: { id: '11111111-1111-1111-1111-111111111111', title: 'The small things that connect us' },
  paragraphs: [
    { id: 'p1', position: 0, sourceText: 'في كل صباح، تفتح المدينة أبوابها لحكايات جديدة. يمشي الناس في شوارعها، ويحمل كلّ منهم أملاً صغيراً ليوم أفضل.', machineDraft: 'Every morning, the city opens its doors to new stories. People walk through its streets, each carrying a small hope for a better day.' },
    { id: 'p2', position: 1, sourceText: 'الكلمات جسور بين الناس. حين نصغي باهتمام، نكتشف أن ما يجمعنا أكبر ممّا يفرّقنا، وأن الفهم يبدأ بسؤال بسيط.', machineDraft: 'Words are bridges between people. When we listen with care, we discover that what unites us is greater than what divides us, and that understanding begins with a simple question.' },
    { id: 'p3', position: 2, sourceText: 'ليست الترجمة نقلاً للكلمات فحسب، بل هي محاولة للحفاظ على المعنى والشعور، وإتاحة الحكاية لقارئ جديد.', machineDraft: 'Translation is more than moving words from one language to another. It is an attempt to preserve meaning and feeling, and to share a story with a new reader.' }
  ].map(p => ({ ...p, reviewedText: null, version: 'sample' }))
};
