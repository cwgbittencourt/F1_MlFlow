window.f1mlflow = window.f1mlflow || {};

window.f1mlflow.downloadFile = (filename, content) => {
    try {
        const blob = new Blob([content], { type: 'text/csv;charset=utf-8;' });
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = filename || 'export.csv';
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        URL.revokeObjectURL(url);
    } catch (error) {
        console.error('Falha ao baixar arquivo', error);
    }
};

window.f1mlflow.renderMermaid = async () => {
    const config = {
        startOnLoad: false,
        securityLevel: 'strict',
        theme: 'dark',
        themeVariables: {
            primaryColor: '#1e293b',
            primaryBorderColor: '#38bdf8',
            primaryTextColor: '#e2e8f0',
            lineColor: '#38bdf8',
            tertiaryColor: '#0b1220',
            background: '#0b1220',
            fontFamily: 'Helvetica, Arial, sans-serif'
        }
    };

    const normalizeSource = (text) => {
        return (text || '')
            .replace(/\r/g, '')
            .replace(/\t/g, '  ')
            .trim();
    };

    const tryRender = (attempt = 0) => {
        if (!window.mermaid) {
            if (attempt < 40) {
                setTimeout(() => tryRender(attempt + 1), 250);
            } else {
                console.warn('Mermaid não carregado.');
            }
            return;
        }

        window.mermaid.initialize(config);
        window.mermaidInitialized = true;

        document.querySelectorAll('pre > code.language-mermaid').forEach((code) => {
            const pre = code.parentElement;
            if (!pre) return;
            const container = document.createElement('div');
            container.className = 'mermaid';
            container.textContent = normalizeSource(code.textContent);
            pre.replaceWith(container);
        });

        const nodes = Array.from(document.querySelectorAll('.mermaid'));
        nodes.forEach((el) => {
            el.removeAttribute('data-processed');
        });

        let counter = 0;
        const renderNode = async (node) => {
            const source = normalizeSource(node.textContent);
            const id = `mmd-${Date.now()}-${counter++}`;
            try {
                if (typeof window.mermaid.parse === 'function') {
                    await window.mermaid.parse(source);
                }
                const result = await window.mermaid.render(id, source);
                node.innerHTML = result.svg;
                if (typeof result.bindFunctions === 'function') {
                    result.bindFunctions(node);
                }
            } catch (error) {
                console.error('Mermaid render error', error, source);
                node.classList.add('mermaid-error');
            }
        };

        Promise.all(nodes.map((node) => renderNode(node))).catch((error) => {
            console.error('Falha ao renderizar Mermaid', error);
        });
    };

    tryRender();
};
