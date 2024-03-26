import * as React from 'react'
import type { SVGProps } from 'react'
const SvgLock = (props: SVGProps<SVGSVGElement>) => (
  <svg xmlns="http://www.w3.org/2000/svg" width="1em" height="1em" fill="none" viewBox="0 0 18 18" {...props}>
    <path stroke="#fff" strokeLinecap="round" strokeWidth={2} d="M14 6v0a5 5 0 0 0-5-5v0a5 5 0 0 0-5 5v0" />
    <path
      fill="#fff"
      fillRule="evenodd"
      d="M1.655 5.956C1 6.776 1 7.983 1 10.398c0 3.218 0 4.828.873 5.922.186.234.399.447.633.633 1.094.873 2.704.873 5.923.873h1.205c3.219 0 4.828 0 5.923-.873.234-.186.446-.399.633-.633.873-1.094.873-2.704.873-5.922 0-2.415 0-3.622-.655-4.442-.14-.176-.3-.335-.475-.475-.82-.655-2.028-.655-4.442-.655h-4.92c-2.414 0-3.62 0-4.441.655a3 3 0 0 0-.475.475Zm7.376 5.792a.677.677 0 1 0 0-1.354.677.677 0 0 0 0 1.354Zm2.678-.677a2.678 2.678 0 0 1-1.678 2.485v1.978h-2v-1.979a2.678 2.678 0 1 1 3.678-2.484Z"
      clipRule="evenodd"
    />
  </svg>
)
export default SvgLock
