window.exportPdf = {
  generate: function (elementId) {
    return new Promise((resolve, reject) => {
      const element = document.getElementById(elementId);
      if (!element) {
        reject("Element not found");
        return;
      }

      // Show element temporarily for capture
      const originalDisplay = element.style.display;
      element.style.display = 'block';

      var opt = {
        margin: 0.5,
        filename: 'journal_export.pdf',
        image: { type: 'jpeg', quality: 0.98 },
        html2canvas: { scale: 2 },
        jsPDF: { unit: 'in', format: 'letter', orientation: 'portrait' }
      };

      // Use html2pdf to generate blob/base64
      html2pdf().set(opt).from(element).outputPdf('datauristring')
        .then(function (pdfAsString) {
          // Hide element again
          element.style.display = originalDisplay;
          // Remove the "data:application/pdf;base64," prefix
          var base64 = pdfAsString.split(',')[1];
          resolve(base64);
        })
        .catch(function (err) {
          element.style.display = originalDisplay;
          reject(err);
        });
    });
  }
};
