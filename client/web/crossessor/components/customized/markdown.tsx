"use client";

import React, { useEffect, useRef } from 'react';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import remarkDirective from 'remark-directive';
import remarkMermaid from 'remark-mermaidjs';
import rehypeRaw from 'rehype-raw';
import rehypeHighlight from 'rehype-highlight';
import mermaid from 'mermaid';
import 'highlight.js/styles/github.css';

interface MarkdownProps {
  content: string;
}

export default function Markdown({ content }: MarkdownProps) {
  const containerRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    mermaid.initialize({ startOnLoad: false, theme: 'default' });
    if (containerRef.current) {
      mermaid.contentLoaded();
    }
  }, [content]);

  return (
    <div ref={containerRef} className="prose prose-neutral dark:prose-invert mx-auto p-4">
      <ReactMarkdown
        remarkPlugins={[remarkGfm, remarkDirective]}
        rehypePlugins={[rehypeRaw, rehypeHighlight]}
      >
        {content}
      </ReactMarkdown>
    </div>
  );
}