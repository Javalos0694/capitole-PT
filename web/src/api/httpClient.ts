import { ApiError, type ProblemDetails } from './types';

const baseUrl = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:8080';

export async function apiRequest<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${baseUrl}${path}`, {
    ...init,
    headers: {
      'Content-Type': 'application/json',
      ...(init?.headers ?? {}),
    },
  });

  if (!response.ok) {
    let problem: ProblemDetails = { title: response.statusText };
    try {
      problem = (await response.json()) as ProblemDetails;
    } catch {
      // keep fallback
    }
    throw new ApiError(problem, response.status);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}
