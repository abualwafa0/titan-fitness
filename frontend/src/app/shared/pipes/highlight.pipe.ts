import { Pipe, PipeTransform } from '@angular/core';

const HTML_ESCAPES: Record<string, string> = {
  '&': '&amp;',
  '<': '&lt;',
  '>': '&gt;',
  '"': '&quot;',
  "'": '&#39;'
};

function escapeHtml(value: string): string {
  return value.replace(/[&<>"']/g, (char) => HTML_ESCAPES[char]);
}

function escapeRegExp(value: string): string {
  return value.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
}

/**
 * Wraps the parts of `text` that match `term` in <mark>.
 * The text is HTML-escaped first, so the result is safe to bind with [innerHTML].
 * Usage: <span [innerHTML]="member.fullName | highlight: searchTerm"></span>
 */
@Pipe({
  name: 'highlight',
  standalone: true
})
export class HighlightPipe implements PipeTransform {
  transform(
    text: string | number | null | undefined,
    term: string | null | undefined
  ): string {
    const value = text === null || text === undefined ? '' : String(text);
    const needle = (term ?? '').trim();

    if (!needle) {
      return escapeHtml(value);
    }

    const regex = new RegExp(escapeRegExp(needle), 'gi');

    let result = '';
    let lastIndex = 0;
    let match: RegExpExecArray | null;

    while ((match = regex.exec(value)) !== null) {
      result += escapeHtml(value.slice(lastIndex, match.index));
      result += `<mark>${escapeHtml(match[0])}</mark>`;
      lastIndex = match.index + match[0].length;

      if (match[0].length === 0) {
        regex.lastIndex++;
      }
    }

    result += escapeHtml(value.slice(lastIndex));

    return result;
  }
}
